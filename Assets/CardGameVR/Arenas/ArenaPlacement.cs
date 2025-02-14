using UnityEngine;
using UnityEngine.Serialization;

namespace CardGameVR.Arenas
{
    public class ArenaPlacement : MonoBehaviour
    {
        private static readonly int IsOccupiedIndex = Animator.StringToHash("IsOccupied");
        private static readonly int IsLocalPlayerIndex = Animator.StringToHash("IsLocalPlayer");
        public Animator animator;

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
            if (!args.Manager.TryGetPlayerData(args.ClientId, out var playerData))
                return;
            if (playerData.Placement != GetPlacementIndex())
                return;
            Debug.Log($"Player {args.ClientId} joined at placement {GetPlacementIndex()}");
            animator.SetBool(IsLocalPlayerIndex, args.Manager.IsPlayerIsLocal(args.ClientId));
            animator.SetBool(IsOccupiedIndex, true);
        }

        private void OnClientLeft(Multiplayer.ClientLeftArgs args)
        {
            if (!args.Manager.TryGetPlayerData(args.ClientId, out var playerData))
                return;
            if (playerData.Placement != GetPlacementIndex())
                return;
            Debug.Log($"Player {args.ClientId} left from placement {GetPlacementIndex()}");
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