using System;
using CardGameVR.Controllers;
using CardGameVR.Players;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace CardGameVR.Arenas
{
    public class ArenaPlacement : MonoBehaviour
    {
        private static readonly int IsOccupiedIndex = Animator.StringToHash("IsOccupied");
        private static readonly int IsLocalPlayerIndex = Animator.StringToHash("IsLocalPlayer");

        [Header("References")] public Animator animator;
        public PlayerAnchor playerAnchor;

        [Header("Network")] public NetworkObject player;
        public ulong clientId = ulong.MaxValue;

        public void Awake()
        {
            clientId = ulong.MaxValue;
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
            Multiplayer.MultiplayerManager.OnClientJoined.AddListener(OnClientJoined);
            Multiplayer.MultiplayerManager.OnClientLeft.AddListener(OnClientLeft);
        }

        private void OnClientJoined(Multiplayer.ClientJoinedArgs args)
        {
            var placementIndex = ArenaDescriptor.GetPlayerIndex(args.ClientId);
            if (placementIndex > -1)
            {
                Debug.Log($"Player {args.ClientId} already joined at placement {placementIndex}");
                return;
            }

            Debug.Log($"Player {args.ClientId} joined at placement {GetPlacementIndex()}");

            if (Multiplayer.MultiplayerManager.IsPlayerIsLocal(args.ClientId) && playerAnchor)
            {
                Debug.Log("Teleport local player to anchor");
                playerAnchor.IsDefault = true;
                ControllerManager.Controller.Teleport(playerAnchor.transform);
            }

            if (Multiplayer.MultiplayerManager.IsServer())
            {
                Debug.Log($"Spawn player for client {args.ClientId}");
                var description = ArenaDescriptor.Instance;
                var playerInstance = Instantiate(description.playerPrefab.gameObject, transform);
                player = playerInstance.GetComponent<NetworkObject>();
                player.SpawnAsPlayerObject(args.ClientId, true);
            }

            clientId = args.ClientId;
            animator.SetBool(IsLocalPlayerIndex, Multiplayer.MultiplayerManager.IsPlayerIsLocal(args.ClientId));
            animator.SetBool(IsOccupiedIndex, true);
        }


        private void OnClientLeft(Multiplayer.ClientLeftArgs args)
        {
            if (clientId != args.ClientId)
                return;

            Debug.Log($"Player {args.ClientId} left from placement {GetPlacementIndex()}");

            if (Multiplayer.MultiplayerManager.IsServer())
            {
                player.Despawn();
                player = null;
            }

            if (Multiplayer.MultiplayerManager.IsPlayerIsLocal(args.ClientId) && playerAnchor)
            {
                playerAnchor.IsDefault = false;
                ControllerManager.Controller.Teleport(playerAnchor.transform);
            }

            clientId = ulong.MaxValue;
            animator.SetBool(IsOccupiedIndex, false);
            animator.SetBool(IsLocalPlayerIndex, false);
        }

        public void OnDestroy()
        {
            Multiplayer.MultiplayerManager.OnClientJoined.RemoveListener(OnClientJoined);
            Multiplayer.MultiplayerManager.OnClientLeft.RemoveListener(OnClientLeft);
        }

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
            UnityEditor.EditorGUILayout.LabelField("Placement Index", placement.GetPlacementIndex().ToString());
        }
    }
#endif
}