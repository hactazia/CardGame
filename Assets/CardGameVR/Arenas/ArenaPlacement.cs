using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        [Header("Lives")] public List<Slider> liveSliders;
        public Slider livePrefab;
        public RectTransform livesContainer;

        [Header("References")] public Animator animator;
        public PlayerAnchor playerAnchor;

        public PlayerNetwork Player
            => PlayerNetwork.Players
                .Find(player => player.PlacementIndex == GetPlacementIndex());

        public void Awake()
        {
            animator.SetBool(IsOccupiedIndex, false);
            animator.SetBool(IsLocalPlayerIndex, false);
        }

        internal int GetPlacementIndex()
        {
            for (var i = 0; i < ArenaDescriptor.Instance.placements.Length; i++)
                if (ArenaDescriptor.Instance.placements[i] == this)
                    return i;
            return -1;
        }

        public void Start()
        {
            readyToggle?.onValueChanged.AddListener(SetReady);
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
            var placementIndex = GetPlacementIndex();
            animator.SetBool(IsYourTurnIndex, placementIndex == turnIndex);
        }

        public void OnDestroy()
        {
            readyToggle?.onValueChanged.RemoveListener(SetReady);
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

            if (Player && silent)
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
            playerNetwork.onLiveChanged.AddListener(OnLiveChanged);
            OnLiveChanged(playerNetwork.Lives);
            OnIsReadyChanged(playerNetwork.IsReady);
            NetworkParty.Instance.onGameStateChange.AddListener(OnGameStateChanged);
            NetworkParty.Instance.onTurnIndexChanged.AddListener(OnTurnIndexChanged);
            OnGameStateChanged(NetworkParty.Instance.IsGameStarted);
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
            UnityEditor.EditorGUILayout.LabelField("Player", placement.Player ? placement.Player.OwnerClientId.ToString() : "None");
            UnityEditor.EditorGUILayout.LabelField("Is Local Player", placement.Player ? placement.Player.IsLocalPlayer.ToString() : "None");
            UnityEditor.EditorGUILayout.LabelField("Is Ready", placement.Player ? placement.Player.IsReady.ToString() : "None");
            UnityEditor.EditorGUILayout.LabelField("Is Game Started", NetworkParty.Instance.IsGameStarted.ToString());
            UnityEditor.EditorGUILayout.LabelField("Is Your Turn", NetworkParty.Instance.TurnIndex == placement.GetPlacementIndex() ? "Yes" : "No");
        }
    }
#endif
}