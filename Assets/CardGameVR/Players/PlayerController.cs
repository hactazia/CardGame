using System;
using CardGameVR.Teams;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace CardGameVR.Players
{
    public class PlayerController : NetworkBehaviour
    {
        public static PlayerController localPlayer { get; private set; }

        // player info
        [SerializeField] private NetworkVariable<string> playerName;

        // player position
        [SerializeField] private NetworkVariable<Vector3> headPosition;
        [SerializeField] private NetworkVariable<Quaternion> headRotation;

        // XR position
        [SerializeField] private NetworkVariable<Vector3> leftHandPosition;
        [SerializeField] private NetworkVariable<Quaternion> leftHandRotation;
        [SerializeField] private NetworkVariable<Vector3> rightHandPosition;
        [SerializeField] private NetworkVariable<Quaternion> rightHandRotation;

        // game state
        [SerializeField] private NetworkVariable<Team> team;
        [SerializeField] private NetworkVariable<PlayerState> playerState;
        [SerializeField] private NetworkVariable<Vector4> levels;

        public float Experience
        {
            get => levels.Value.x;
            set => levels.Value = new Vector4(value, levels.Value.y, levels.Value.z, levels.Value.w);
        }

        public float Mana
        {
            get => levels.Value.y;
            set => levels.Value = new Vector4(levels.Value.x, value, levels.Value.z, levels.Value.w);
        }

        public float Power
        {
            get => levels.Value.z;
            set => levels.Value = new Vector4(levels.Value.x, levels.Value.y, value, levels.Value.w);
        }

        public float Endurance
        {
            get => levels.Value.w;
            set => levels.Value = new Vector4(levels.Value.x, levels.Value.y, levels.Value.z, value);
        }

        public static readonly LevelChangedEvent OnLevelChanged = new();
        public static readonly TeamChangedEvent OnTeamChanged = new();
        public static readonly PlayerStateChangedEvent OnStateChanged = new();
        public static readonly PlayerNameChangedEvent OnPlayerNameChanged = new();
        public static readonly TransformEvent OnHeadTransformChanged = new();
        public static readonly TransformEvent OnLeftHandTransformChanged = new();
        public static readonly TransformEvent OnRightHandTransformChanged = new();

        private void Awake()
        {
            // Player State
            playerState = new NetworkVariable<PlayerState>(PlayerState.WaitingForTurn);
            playerState.OnValueChanged += PlayerState_OnValueChanged;

            // Levels (Experience, Mana, Power, Endurance)
            levels = new NetworkVariable<Vector4>(new Vector4(.5f, .5f, .5f, .5f));
            levels.OnValueChanged += Level_OnValueChanged;

            // Team
            team = new NetworkVariable<Team>(Team.None);
            team.OnValueChanged += Team_OnValueChanged;

            // Player Name
            playerName = new NetworkVariable<string>("Player");
            playerName.OnValueChanged += PlayerName_OnValueChanged;

            // Head Position
            headPosition = new NetworkVariable<Vector3>(Vector3.zero);
            headPosition.OnValueChanged += HeadTransform_OnValueChanged;
            headRotation = new NetworkVariable<Quaternion>(Quaternion.identity);
            headRotation.OnValueChanged += HeadTransform_OnValueChanged;

            // Left Hand Position
            leftHandPosition = new NetworkVariable<Vector3>(Vector3.zero);
            leftHandPosition.OnValueChanged += LeftHandTransform_OnValueChanged;
            leftHandRotation = new NetworkVariable<Quaternion>(Quaternion.identity);
            leftHandRotation.OnValueChanged += LeftHandTransform_OnValueChanged;

            // Right Hand Position
            rightHandPosition = new NetworkVariable<Vector3>(Vector3.zero);
            rightHandPosition.OnValueChanged += RightHandTransform_OnValueChanged;
            rightHandRotation = new NetworkVariable<Quaternion>(Quaternion.identity);
            rightHandRotation.OnValueChanged += RightHandTransform_OnValueChanged;
        }

        private void Start()
        {
        }

        private void Team_OnValueChanged(Team previousValue, Team newValue)
            => OnTeamChanged.Invoke(new TeamChangedArgs
                { Player = this, OldTeam = previousValue, NewTeam = newValue });

        private void Level_OnValueChanged(Vector4 previousValue, Vector4 newValue)
            => OnLevelChanged.Invoke(new LevelChangedArgs
                { Player = this, OldLevels = previousValue, NewLevels = newValue });

        private void PlayerState_OnValueChanged(PlayerState previousValue, PlayerState newValue)
            => OnStateChanged.Invoke(new PlayerStateChangedArgs
                { Player = this, OldState = previousValue, NewState = newValue });

        private void PlayerName_OnValueChanged(string previousValue, string newValue)
            => OnPlayerNameChanged.Invoke(new PlayerNameChangedArgs
                { Player = this, OldName = previousValue, NewName = newValue });


        private void HeadTransform_OnValueChanged(Vector3 previousValue, Vector3 newValue)
            => OnHeadTransformChanged.Invoke(new TransformArgs
                { Player = this, Position = headPosition.Value, Rotation = headRotation.Value });

        private void HeadTransform_OnValueChanged(Quaternion previousValue, Quaternion newValue)
            => OnHeadTransformChanged.Invoke(new TransformArgs
                { Player = this, Position = headPosition.Value, Rotation = headRotation.Value });

        private void LeftHandTransform_OnValueChanged(Vector3 previousValue, Vector3 newValue)
            => OnLeftHandTransformChanged.Invoke(new TransformArgs
                { Player = this, Position = leftHandPosition.Value, Rotation = leftHandRotation.Value });

        private void LeftHandTransform_OnValueChanged(Quaternion previousValue, Quaternion newValue)
            => OnLeftHandTransformChanged.Invoke(new TransformArgs
                { Player = this, Position = leftHandPosition.Value, Rotation = leftHandRotation.Value });

        private void RightHandTransform_OnValueChanged(Vector3 previousValue, Vector3 newValue)
            => OnRightHandTransformChanged.Invoke(new TransformArgs
                { Player = this, Position = rightHandPosition.Value, Rotation = rightHandRotation.Value });

        private void RightHandTransform_OnValueChanged(Quaternion previousValue, Quaternion newValue)
            => OnRightHandTransformChanged.Invoke(new TransformArgs
                { Player = this, Position = rightHandPosition.Value, Rotation = rightHandRotation.Value });
    }
}