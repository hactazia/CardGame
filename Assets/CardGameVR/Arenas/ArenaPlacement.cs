using System.Linq;
using CardGameVR.Cards.Groups;
using CardGameVR.Controllers;
using CardGameVR.Parties;
using CardGameVR.Players;
using UnityEngine;
using UnityEngine.UI;

namespace CardGameVR.Arenas
{
    public class ArenaPlacement : MonoBehaviour
    {
        // Class Functions
        public static ArenaPlacement Get(int value)
            => value >= 0 && value < ArenaDescriptor.Instance.placements.Length
                ? ArenaDescriptor.Instance.placements[value]
                : null;

        public static ArenaPlacement Get(NetworkPlayer player)
            => player ? Get(player.Placement) : null;

        public static ArenaPlacement Get()
            => ArenaDescriptor.Instance.placements
                .FirstOrDefault(placement => !placement.Player);

        // Class Variables

        private static readonly int IsOccupiedIndex = Animator.StringToHash("IsOccupied");
        private static readonly int IsLocalPlayerIndex = Animator.StringToHash("IsLocalPlayer");
        private static readonly int IsReadyIndex = Animator.StringToHash("IsReady");
        private static readonly int IsGameStartedIndex = Animator.StringToHash("IsGameStarted");
        private static readonly int IsYourTurnIndex = Animator.StringToHash("IsYourTurn");

        // Instance Variables

        public Animator animator;
        public PlayerAnchor playerAnchor;
        public HandGroup hand;

        // Instance Functions

        public int GetIndex()
        {
            if (!ArenaDescriptor.Instance) return -1;
            for (var i = 0; i < ArenaDescriptor.Instance.placements.Length; i++)
                if (ArenaDescriptor.Instance.placements[i] == this)
                    return i;
            return -1;
        }

        public NetworkPlayer Player
            => NetworkPlayer.Players
                .Find(player => player.Placement == GetIndex());

        void Start()
        {
            NetworkPlayer.OnPlayerJoined.AddListener(player_OnPlayerJoined);
            NetworkPlayer.OnPlacement.AddListener(player_OnPlacement);
            NetworkPlayer.OnPlayerLeft.AddListener(player_OnPlayerLeft);
            NetworkParty.OnState.AddListener(party_OnState);
            NetworkParty.OnTurn.AddListener(player_OnTurn);
            SetDefaultValues();
        }

        void OnDestroy()
        {
            NetworkPlayer.OnPlayerJoined.RemoveListener(player_OnPlayerJoined);
            NetworkPlayer.OnPlacement.RemoveListener(player_OnPlacement);
            NetworkPlayer.OnPlayerLeft.RemoveListener(player_OnPlayerLeft);
            NetworkParty.OnState.RemoveListener(party_OnState);
            NetworkParty.OnTurn.RemoveListener(player_OnTurn);
        }


        // Event Listeners

        private void party_OnState(bool isGameStarted)
        {
            animator.SetBool(IsGameStartedIndex, isGameStarted);
        }

        private void player_OnTurn(NetworkPlayer player)
        {
            if (player.Placement != GetIndex()) return;
            animator.SetBool(IsYourTurnIndex, player.IsMyTurn);
        }

        private void player_OnPlayerJoined(NetworkPlayer player)
        {
            if (player.Placement == -1 && player.IsLocalPlayer)
                player.Placement = Get().GetIndex();
            if (player.Placement != GetIndex()) return;
            SetCatapultedValues();
            if (player.IsLocalPlayer)
                MovePlayerHere();
        }

        private void player_OnPlacement(NetworkPlayer player, int oldPlacement)
        {
            if (oldPlacement == GetIndex())
                SetDefaultValues();

            if (player.Placement == GetIndex())
            {
                SetCatapultedValues();
                if (player.IsLocalPlayer)
                    MovePlayerHere();
            }
        }

        private void player_OnPlayerLeft(NetworkPlayer player)
        {
            if (player.Placement != GetIndex()) return;
            SetDefaultValues();
        }

        /*private void button_OnDrawClick()
        {
            if (!Player || !Player.IsLocalPlayer) return;

            if (!Player.IsMyTurn)
            {
                Debug.LogError("Not your turn");
                return;
            }

            if (Player.Hand.Length >= ArenaDescriptor.Instance.partyConfiguration.maxCardInHand)
                Player.ClearHand();
            Player.DrawCard();
        }*/

        private void SetDefaultValues()
        {
            Debug.Log($"Set default values for placement {GetIndex()}");
            animator.SetBool(IsOccupiedIndex, false);
            animator.SetBool(IsLocalPlayerIndex, false);
            animator.SetBool(IsReadyIndex, false);
            animator.SetBool(IsGameStartedIndex, false);
            animator.SetBool(IsYourTurnIndex, false);
        }

        private void SetCatapultedValues()
        {
            Debug.Log($"Set catapulted values for placement {GetIndex()}");
            animator.SetBool(IsOccupiedIndex, true);
            animator.SetBool(IsLocalPlayerIndex, Player.IsLocalPlayer);
            animator.SetBool(IsReadyIndex, Player.Ready);
            animator.SetBool(IsGameStartedIndex, NetworkParty.Instance?.IsStarted ?? false);
            animator.SetBool(IsYourTurnIndex, Player.IsMyTurn);
        }

        private void MovePlayerHere()
        {
            Debug.Log("Teleport local player to anchor");
            playerAnchor.IsDefault = true;
            ControllerManager.Controller.Teleport(playerAnchor.transform);
        }


/*
        private void UpdateInteraction()
        {
            drawButton.interactable = IsYourTurn;
            handListener.OnSlotUpdates();
            if (NetworkPlayer.IsLocalPlayer)
                boardListener.OnSlotUpdates();
        }

        private void OnGameStateChanged(bool isGameStarted)
        {
            Debug.Log("Game state changed to: " + isGameStarted);
            animator.SetBool(IsGameStartedIndex, isGameStarted);
            UpdateInteraction();
        }

        private void OnTurnIndexChanged(int turnIndex)
        {
            animator.SetBool(IsYourTurnIndex, IsYourTurn);
            UpdateInteraction();
        }

        private void OnPlayerCardAdded(HandCard card)
            => OnPlayerCardAddedAsync(card).Forget();

        private async UniTask OnPlayerCardAddedAsync(HandCard card)
        {
            Debug.Log($"Add card: {card.Id}");
            var slot = handCardGroup.slots.Find(e => e.Card.GetId() == card.Id);
            if (slot)
            {
                Debug.LogError("Card already exists in hand");
                return;
            }

            var spawned = await card.Spawn();
            var visual = !spawned.TryGetVisualCard(out var cardVisual)
                ? spawned.SpawnVisualCard(handCardGroup.visualCardHandler ?? VisualCardHandler.Instance)
                : cardVisual;
            visual.transform.position = drawButton.transform.position;
            visual.transform.rotation = drawButton.transform.rotation;
            handCardGroup.Add(spawned);
            slot = handCardGroup.slots.Find(e => e.Card.GetId() == card.Id);
            slot.interactable = IsYourTurn;
            handListener.OnSlotUpdate(slot);
        }

        private void OnPlayerCardRemoved(HandCard card)
            => OnPlayerCardRemovedAsync(card).Forget();

        private async UniTask OnPlayerCardRemovedAsync(HandCard card)
        {
            Debug.Log($"Remove card: {card.Id}");
            var slot = handCardGroup.slots.Find(e => e.Card.GetId() == card.Id);
            if (!slot)
            {
                Debug.LogError("Card does not exist in hand");
                return;
            }

            Debug.Log($"Remove card: {card.Id} 0");
            await UniTask.Yield();
            Debug.Log($"Remove card: {card.Id} 1");
            handCardGroup.Remove(slot);
        }

        private void OnPlayerCardCleared()
        {
            Debug.Log($"Player card cleared");
            handCardGroup.Clear();
        }

        private void SetReady(bool ready)
        {
            if (!NetworkPlayer || !NetworkPlayer.IsLocalPlayer || ready == NetworkPlayer.IsReady) return;
            NetworkPlayer.IsReady = ready;
        }

        public async UniTask SetPlayer(NetworkPlayer networkPlayer, bool silent = false)
        {
            await NetworkParty.WhenIsInstanced();

            if (!networkPlayer)
            {
                if (!NetworkPlayer)
                {
                    Debug.LogError("Placement is already empty.");
                    return;
                }

                NetworkPlayer.onIsReadyChanged.RemoveListener(OnIsReadyChanged);
                NetworkPlayer.onLiveChanged.RemoveListener(OnLiveChanged);
                NetworkPlayer.onPlayerCardAdded.RemoveListener(OnPlayerCardAdded);
                NetworkPlayer.onPlayerCardRemoved.RemoveListener(OnPlayerCardRemoved);
                NetworkPlayer.onPlayerCardCleared.RemoveListener(OnPlayerCardCleared);
                NetworkParty.Instance.OnGameStateChange.RemoveListener(OnGameStateChanged);
                NetworkParty.Instance.OnTurnIndexChanged.RemoveListener(OnTurnIndexChanged);
                if (!silent) NetworkPlayer.PlacementIndex = -1;
                animator.SetBool(IsLocalPlayerIndex, false);
                animator.SetBool(IsOccupiedIndex, false);
                animator.SetBool(IsReadyIndex, false);
                animator.SetBool(IsGameStartedIndex, false);
                animator.SetBool(IsYourTurnIndex, false);
                return;
            }

            if (NetworkPlayer && NetworkPlayer.OwnerClientId == networkPlayer.OwnerClientId && silent)
            {
                await SetPlayer(null, true);
            }
            else if (NetworkPlayer)
            {
                Debug.LogError("Placement is already occupied.");
                return;
            }

            Debug.Log($"Set Placement {GetPlacementIndex()} to player {networkPlayer.OwnerClientId}");

            if (!silent)
                networkPlayer.PlacementIndex = GetPlacementIndex();

            networkPlayer.transform.position = transform.position;
            networkPlayer.transform.rotation = transform.rotation;
            animator.SetBool(IsOccupiedIndex, true);
            animator.SetBool(IsLocalPlayerIndex, networkPlayer.IsLocalPlayer);

            networkPlayer.onIsReadyChanged.AddListener(OnIsReadyChanged);
            OnIsReadyChanged(networkPlayer.IsReady);

            networkPlayer.onLiveChanged.AddListener(OnLiveChanged);
            OnLiveChanged(networkPlayer.Lives);

            networkPlayer.onPlayerCardAdded.AddListener(OnPlayerCardAdded);
            networkPlayer.onPlayerCardRemoved.AddListener(OnPlayerCardRemoved);
            networkPlayer.onPlayerCardCleared.AddListener(OnPlayerCardCleared);
            OnPlayerCardCleared();
            foreach (var card in networkPlayer.Hand)
                await OnPlayerCardAddedAsync(card);

            UpdateInteraction();

            NetworkParty.Instance.OnGameStateChange.AddListener(OnGameStateChanged);
            OnGameStateChanged(NetworkParty.Instance.IsGameStarted);

            NetworkParty.Instance.OnTurnIndexChanged.AddListener(OnTurnIndexChanged);
            OnTurnIndexChanged(NetworkParty.Instance.TurnIndex);

            if (Multiplayer.MultiplayerManager.IsPlayerIsLocal(networkPlayer.OwnerClientId) && playerAnchor)
            {
                Debug.Log("Teleport local player to anchor");
                playerAnchor.IsDefault = true;
                ControllerManager.Controller.Teleport(playerAnchor.transform);
                readyToggle.isOn = networkPlayer.IsReady;
            }
        }
*/

#if UNITY_EDITOR
        public void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, 0.1f);
        }
#endif
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
            UnityEditor.EditorGUILayout.LabelField("Placement Index", placement.GetIndex().ToString());
            UnityEditor.EditorGUILayout.LabelField("Player",
                placement.Player ? placement.Player.OwnerClientId.ToString() : "None");
            UnityEditor.EditorGUILayout.LabelField("Is Local Player",
                placement.Player ? placement.Player.IsLocalPlayer.ToString() : "None");
            UnityEditor.EditorGUILayout.LabelField("Is Ready",
                placement.Player ? placement.Player.Ready.ToString() : "None");
            UnityEditor.EditorGUILayout.LabelField("Is Your Turn",
                placement.Player ? placement.Player.IsMyTurn.ToString() : "None");
            UnityEditor.EditorGUILayout.Space();
            var cards = placement.hand.GetCards();
            UnityEditor.EditorGUILayout.LabelField("Hand Cards", cards.Length.ToString());
            foreach (var card in cards)
                UnityEditor.EditorGUILayout.LabelField($"(${card.GetCardType()}) {card.GetId()}");
        }
    }
#endif
}