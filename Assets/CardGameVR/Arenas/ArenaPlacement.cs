using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CardGameVR.Cards;
using CardGameVR.Cards.Groups;
using CardGameVR.Cards.Visual;
using CardGameVR.Controllers;
using CardGameVR.Players;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace CardGameVR.Arenas
{
    public class ArenaPlacement : MonoBehaviour
    {
        private static readonly int IsOccupiedIndex = Animator.StringToHash("IsOccupied");
        private static readonly int IsLocalPlayerIndex = Animator.StringToHash("IsLocalPlayer");
        private static readonly int IsReadyIndex = Animator.StringToHash("IsReady");
        private static readonly int IsGameStartedIndex = Animator.StringToHash("IsGameStarted");
        private static readonly int IsYourTurnIndex = Animator.StringToHash("IsYourTurn");

        public Toggle readyToggle;
        public Button drawButton;

        [Header("Lives")] public List<Slider> liveSliders;
        public Slider livePrefab;
        public RectTransform livesContainer;

        [Header("References")] public Animator animator;
        public PlayerAnchor playerAnchor;

        public PlayerNetwork Player
            => PlayerNetwork.Players
                .Find(player => player.PlacementIndex == GetPlacementIndex());

        public HorizontalCardGroup handCardGroup;

        internal int GetPlacementIndex()
        {
            for (var i = 0; i < ArenaDescriptor.Instance.placements.Length; i++)
                if (ArenaDescriptor.Instance.placements[i] == this)
                    return i;
            return -1;
        }

        public void Start()
        {
            animator?.SetBool(IsOccupiedIndex, false);
            animator?.SetBool(IsLocalPlayerIndex, false);
            animator?.SetBool(IsReadyIndex, false);
            animator?.SetBool(IsGameStartedIndex, false);
            animator?.SetBool(IsYourTurnIndex, false);
            readyToggle?.onValueChanged.AddListener(SetReady);
            drawButton?.onClick.AddListener(OnDrawClick);
        }

        private void OnIsReadyChanged(bool isReady)
        {
            animator.SetBool(IsReadyIndex, isReady);
            NetworkParty.Instance.CheckAllPlayersReady();
        }

        private void OnGameStateChanged(bool isGameStarted)
            => animator.SetBool(IsGameStartedIndex, isGameStarted);

        private void OnTurnIndexChanged(int turnIndex)
        {
            animator.SetBool(IsYourTurnIndex, IsYourTurn);

            drawButton.interactable = IsYourTurn;
            foreach (var slot in handCardGroup.slots)
                slot.interactable = IsYourTurn;
            foreach (var slot in ArenaDescriptor.Instance.gameBoard.slots)
                slot.interactable = IsYourTurn;
        }
        
        public bool IsYourTurn => GetPlacementIndex() == NetworkParty.Instance.TurnIndex && Player && Player.IsLocalPlayer && NetworkParty.Instance.IsGameStarted;

        private void OnPlayerCardAdded(PlayerHandCard playerCard)
        {
            Debug.Log($"OnPlayerCardAdded({playerCard.Id})");
            OnPlayerCardAddedAsync(playerCard).Forget();
        }

        private async UniTask OnPlayerCardAddedAsync(PlayerHandCard playerCard)
        {
            var slot = handCardGroup.slots.Find(e => e.Card.GetId() == playerCard.Id);
            if (slot)
            {
                Debug.LogError("Card already exists in hand");
                return;
            }

            var spawned = await playerCard.Spawn();
            var visual = !spawned.TryGetVisualCard(out var cardVisual)
                ? spawned.SpawnVisualCard(handCardGroup.visualCardHandler ?? VisualCardHandler.Instance)
                : cardVisual;
            visual.transform.position = drawButton.transform.position;
            visual.transform.rotation = drawButton.transform.rotation;
            handCardGroup.Add(spawned);
            slot = handCardGroup.slots.Find(e => e.Card.GetId() == playerCard.Id);

            slot.interactable = IsYourTurn;
        }

        private void OnPlayerCardRemoved(PlayerHandCard playerCard)
        {
            var slot = handCardGroup.slots.Find(e => e.Card.GetId() == playerCard.Id);
            if (!slot)
            {
                Debug.LogError("Card does not exist in hand");
                return;
            }

            handCardGroup.Remove(slot);
        }

        private void OnPlayerCardUpdated(PlayerHandCard playerCard)
        {
            var slot = handCardGroup.slots.Find(e => e.Card.GetId() == playerCard.Id);
            if (!slot)
            {
                Debug.LogError("Card does not exist in hand");
                return;
            }
        }

        private void OnPlayerCardCleared()
        {
            Debug.Log($"OnPlayerCardCleared");
            handCardGroup.Clear();
        }

        public void OnDestroy()
        {
            readyToggle?.onValueChanged.RemoveListener(SetReady);
            drawButton?.onClick.RemoveListener(OnDrawClick);
        }

#if UNITY_EDITOR
        public void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, 0.1f);
        }
#endif

        private void SetReady(bool ready)
        {
            if (!Player || !Player.IsLocalPlayer || ready == Player.IsReady) return;
            Player.IsReady = ready;
        }

        public async UniTask SetPlayer(PlayerNetwork playerNetwork, bool silent = false)
        {
            await NetworkParty.WhenIsInstanced();

            if (!playerNetwork)
            {
                if (!Player)
                {
                    Debug.LogError("Placement is already empty.");
                    return;
                }

                Player.onIsReadyChanged.RemoveListener(OnIsReadyChanged);
                Player.onLiveChanged.RemoveListener(OnLiveChanged);
                Player.onPlayerCardAdded.RemoveListener(OnPlayerCardAdded);
                Player.onPlayerCardRemoved.RemoveListener(OnPlayerCardRemoved);
                Player.onPlayerCardUpdated.RemoveListener(OnPlayerCardUpdated);
                Player.onPlayerCardCleared.RemoveListener(OnPlayerCardCleared);
                NetworkParty.Instance.onGameStateChange.RemoveListener(OnGameStateChanged);
                NetworkParty.Instance.onTurnIndexChanged.RemoveListener(OnTurnIndexChanged);
                if (!silent) Player.PlacementIndex = -1;
                animator.SetBool(IsLocalPlayerIndex, false);
                animator.SetBool(IsOccupiedIndex, false);
                animator.SetBool(IsReadyIndex, false);
                animator.SetBool(IsGameStartedIndex, false);
                animator.SetBool(IsYourTurnIndex, false);
                return;
            }

            if (Player && Player.OwnerClientId == playerNetwork.OwnerClientId && silent)
            {
                await SetPlayer(null, true);
            }
            else if (Player)
            {
                Debug.LogError("Placement is already occupied.");
                return;
            }

            Debug.Log($"Set Placement {GetPlacementIndex()} to player {playerNetwork.OwnerClientId}");

            if (!silent)
                playerNetwork.PlacementIndex = GetPlacementIndex();

            playerNetwork.transform.position = transform.position;
            playerNetwork.transform.rotation = transform.rotation;
            animator.SetBool(IsOccupiedIndex, true);
            animator.SetBool(IsLocalPlayerIndex, playerNetwork.IsLocalPlayer);

            playerNetwork.onIsReadyChanged.AddListener(OnIsReadyChanged);
            OnIsReadyChanged(playerNetwork.IsReady);

            playerNetwork.onLiveChanged.AddListener(OnLiveChanged);
            OnLiveChanged(playerNetwork.Lives);

            playerNetwork.onPlayerCardAdded.AddListener(OnPlayerCardAdded);
            playerNetwork.onPlayerCardRemoved.AddListener(OnPlayerCardRemoved);
            playerNetwork.onPlayerCardUpdated.AddListener(OnPlayerCardUpdated);
            playerNetwork.onPlayerCardCleared.AddListener(OnPlayerCardCleared);
            OnPlayerCardCleared();
            foreach (var card in playerNetwork.Hand)
                await OnPlayerCardAddedAsync(card);

            NetworkParty.Instance.onGameStateChange.AddListener(OnGameStateChanged);
            OnGameStateChanged(NetworkParty.Instance.IsGameStarted);

            NetworkParty.Instance.onTurnIndexChanged.AddListener(OnTurnIndexChanged);
            OnTurnIndexChanged(NetworkParty.Instance.TurnIndex);

            if (Multiplayer.MultiplayerManager.IsPlayerIsLocal(playerNetwork.OwnerClientId) && playerAnchor)
            {
                Debug.Log("Teleport local player to anchor");
                playerAnchor.IsDefault = true;
                ControllerManager.Controller.Teleport(playerAnchor.transform);
                readyToggle.isOn = playerNetwork.IsReady;
            }
        }

        private void OnLiveChanged(float[] arg0)
        {
            for (var i = 0; i < liveSliders.Count; i++)
            {
                if (i >= arg0.Length)
                {
                    liveSliders[i].gameObject.SetActive(false);
                    continue;
                }

                liveSliders[i].gameObject.SetActive(true);
                liveSliders[i].value = arg0[i];
            }

            for (var i = liveSliders.Count; i < arg0.Length; i++)
            {
                var instance = Instantiate(livePrefab.gameObject, livesContainer);
                var slider = instance.GetComponent<Slider>();
                slider.value = arg0[i];
                liveSliders.Add(slider);
            }
        }

        private void OnDrawClick()
        {
            if (!Player || !Player.IsLocalPlayer) return;


            if (NetworkParty.Instance.TurnIndex != GetPlacementIndex())
            {
                Debug.LogError("Not your turn");
                return;
            }

            if (Player.GetHandCards().Length >= ArenaDescriptor.Instance.partyConfiguration.maxCardInHand)
                Player.ClearCards();

            Player.DrawCard();
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(ArenaPlacement))]
    public class ArenaPlacementEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (!Application.isPlaying) return;
            var placement = (ArenaPlacement)target;
            UnityEditor.EditorGUILayout.LabelField("Placement Index", placement.GetPlacementIndex().ToString());
            UnityEditor.EditorGUILayout.LabelField("Player",
                placement.Player ? placement.Player.OwnerClientId.ToString() : "None");
            UnityEditor.EditorGUILayout.LabelField("Is Local Player",
                placement.Player ? placement.Player.IsLocalPlayer.ToString() : "None");
            UnityEditor.EditorGUILayout.LabelField("Is Ready",
                placement.Player ? placement.Player.IsReady.ToString() : "None");
            UnityEditor.EditorGUILayout.LabelField("Is Game Started", NetworkParty.Instance.IsGameStarted.ToString());
            UnityEditor.EditorGUILayout.LabelField("Is Your Turn",
                NetworkParty.Instance.TurnIndex == placement.GetPlacementIndex() ? "Yes" : "No");
            UnityEditor.EditorGUILayout.Space();
            var cards = placement.handCardGroup.GetCards();
            UnityEditor.EditorGUILayout.LabelField("Hand Cards", cards.Length.ToString());
            foreach (var card in cards)
                UnityEditor.EditorGUILayout.LabelField($"(${card.GetCardType()}) {card.GetId()}");
        }
    }
#endif
}