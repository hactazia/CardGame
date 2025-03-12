using System;
using System.Collections.Generic;
using System.Linq;
using CardGameVR.API;
using CardGameVR.Arenas;
using CardGameVR.Controllers;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace CardGameVR.Players
{
    public class PlayerNetwork : NetworkBehaviour
    {
        public static List<PlayerNetwork> Players { get; } = new();

        public static PlayerNetwork GetPlayer(ulong clientId)
            => Players.FirstOrDefault(player => player.OwnerClientId == clientId);

        public static async UniTask<PlayerNetwork> WhenPlayerSpawned(ulong clientId)
        {
            while (GetPlayer(clientId) is null)
                await UniTask.Yield();
            return GetPlayer(clientId);
        }

        public Transform head;
        public Transform leftHand;
        public Transform rightHand;

        public UnityEvent<string> onPlayerNameChanged = new();
        public UnityEvent<string> onPlayerIdChanged = new();
        public UnityEvent<bool> onIsReadyChanged = new();
        public UnityEvent<int, int> onPlacementIndexChanged = new();

        // life is an array of different life progress (between -1 and 1) 
        // is one of life progress is under -1 or above 1, the player is dead
        public UnityEvent<float[]> onLiveChanged = new();

        public void Awake()
        {
            Players.Add(this);
            _playerName.OnValueChanged += (_, newValue)
                => onPlayerNameChanged.Invoke(newValue.ToString());
            _playerId.OnValueChanged += (_, newValue)
                => onPlayerIdChanged.Invoke(newValue.ToString());
            _isReady.OnValueChanged += (_, newValue)
                => onIsReadyChanged.Invoke(newValue);
            _placementIndex.OnValueChanged += (oldValue, newValue)
                => onPlacementIndexChanged.Invoke(oldValue, newValue);
            _lives.OnListChanged += OnLiveChanged;
        }

        private void OnLiveChanged(NetworkListEvent<float> changeEvent)
        {
            var floats = new float[_lives.Count];
            for (var i = 0; i < _lives.Count; i++)
                floats[i] = _lives[i];
            onLiveChanged.Invoke(floats);
        }

        private void OnPlacementIndexChanged(int oldValue, int newValue)
        {
            Debug.Log($"OnPlacementIndexChanged {oldValue} -> {newValue}");
            if (oldValue != -1)
                ArenaDescriptor.Instance.GetPlacement(oldValue)?.SetPlayer(null);
            if (newValue != -1)
                ArenaDescriptor.Instance.GetPlacement(newValue)?.SetPlayer(this);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            Players.Remove(this);
        }

        public string PlayerName
        {
            get => _playerName.Value.ToString();
            set
            {
                var fixedBytes = new FixedString128Bytes(value);
                if (IsServer)
                    _playerName.Value = fixedBytes;
                else SetPlayerNameServerRpc(fixedBytes);
            }
        }

        public string PlayerId
        {
            get => _playerId.Value.ToString();
            set
            {
                var fixedBytes = new FixedString128Bytes(value);
                if (IsServer)
                    _playerId.Value = fixedBytes;
                else SetPlayerIdServerRpc(fixedBytes);
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsOwner)
            {
                PlayerName = BaseAPI.GetDisplayName();
                PlayerId = BaseAPI.GetId();
            }

            Debug.Log(
                $"PlayerNetwork has as {OwnerClientId} (IsOwner: {IsOwner}) {PlayerName} {PlayerId} {IsReady} {PlacementIndex}");
            onPlacementIndexChanged.AddListener(OnPlacementIndexChanged);

            ArenaDescriptor.Instance.GetPlacement(PlacementIndex)
                ?.SetPlayer(this, true);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            onPlacementIndexChanged.RemoveListener(OnPlacementIndexChanged);
        }

        [ServerRpc]
        private void SetPlayerNameServerRpc(FixedString128Bytes playerName)
            => _playerName.Value = playerName;

        [ServerRpc]
        private void SetPlayerIdServerRpc(FixedString128Bytes id)
            => _playerName.Value = id;

        public bool IsReady
        {
            get => _isReady.Value;
            set
            {
                if (!IsOwner) return;
                if (IsServer)
                    _isReady.Value = value;
                else SetPlayerReadyServerRpc(value);
            }
        }

        public static int PlayerCount => Players.Count;

        [ServerRpc]
        private void SetPlayerReadyServerRpc(bool isReady)
            => _isReady.Value = isReady;


        public int PlacementIndex
        {
            get => _placementIndex.Value;
            set
            {
                if (!IsOwner) return;
                if (IsServer)
                    _placementIndex.Value = value;
                else SetPlacementIndexServerRpc(value);
            }
        }

        [ServerRpc]
        private void SetPlacementIndexServerRpc(int index)
            => _placementIndex.Value = index;

        public float[] Lives
        {
            get
            {
                var floats = new float[_lives.Count];
                for (var i = 0; i < _lives.Count; i++)
                    floats[i] = _lives[i];
                return floats;
            }
            set
            {
                if (!IsOwner) return;
                if (IsServer)
                {
                    _lives.Clear();
                    foreach (var f in value)
                        _lives.Add(f);
                }
                else SetLifeServerRpc(value);
            }
        }

        [ServerRpc]
        private void SetLifeServerRpc(float[] lives)
        {
            _lives.Clear();
            foreach (var f in lives)
                _lives.Add(f);
        }


        private void Update()
        {
            if (!IsOwner) return;
            var player = ControllerManager.Controller;
            if (!player) return;

            var hasRightHand = player.TryGetTransform(HumanBodyBones.RightHand, out var tRightHand);
            var hasLeftHand = player.TryGetTransform(HumanBodyBones.LeftHand, out var tLeftHand);
            player.TryGetTransform(HumanBodyBones.Head, out var tHead);

            head.position = tHead?.position ?? Vector3.zero;
            head.rotation = tHead?.rotation ?? Quaternion.identity;
            leftHand.position = tLeftHand?.position ?? Vector3.zero;
            leftHand.rotation = tLeftHand?.rotation ?? Quaternion.identity;
            rightHand.position = tRightHand?.position ?? Vector3.zero;
            rightHand.rotation = tRightHand?.rotation ?? Quaternion.identity;
            rightHand.gameObject.SetActive(hasRightHand);
            leftHand.gameObject.SetActive(hasLeftHand);

            UpdateNetworkTransformsServerRpc(
                head.position, head.rotation,
                leftHand.position, leftHand.rotation,
                rightHand.position, rightHand.rotation,
                hasRightHand, hasLeftHand
            );
        }

        [ServerRpc]
        private void UpdateNetworkTransformsServerRpc(
            Vector3 headPos, Quaternion headRot,
            Vector3 leftPos, Quaternion leftRot,
            Vector3 rightPos, Quaternion rightRot,
            bool hasRightHand = false, bool hasLeftHand = false
        )
        {
            _headPosition.Value = headPos;
            _headRotation.Value = headRot;
            _leftHandPosition.Value = leftPos;
            _leftHandRotation.Value = leftRot;
            _rightHandPosition.Value = rightPos;
            _rightHandRotation.Value = rightRot;
            _hasRightHand.Value = hasRightHand;
            _hasLeftHand.Value = hasLeftHand;
        }

        private readonly NetworkVariable<Vector3> _headPosition = new();
        private readonly NetworkVariable<Quaternion> _headRotation = new();
        private readonly NetworkVariable<Vector3> _leftHandPosition = new();
        private readonly NetworkVariable<Quaternion> _leftHandRotation = new();
        private readonly NetworkVariable<Vector3> _rightHandPosition = new();
        private readonly NetworkVariable<Quaternion> _rightHandRotation = new();
        private readonly NetworkVariable<bool> _hasRightHand = new();
        private readonly NetworkVariable<bool> _hasLeftHand = new();
        private readonly NetworkVariable<FixedString128Bytes> _playerName = new();
        private readonly NetworkVariable<FixedString128Bytes> _playerId = new();
        private readonly NetworkVariable<bool> _isReady = new();
        private readonly NetworkVariable<int> _placementIndex = new();
        private readonly NetworkList<float> _lives = new();

        private void LateUpdate()
        {
            if (IsOwner) return;
            head.position = _headPosition.Value;
            head.rotation = _headRotation.Value;
            leftHand.position = _leftHandPosition.Value;
            leftHand.rotation = _leftHandRotation.Value;
            rightHand.position = _rightHandPosition.Value;
            rightHand.rotation = _rightHandRotation.Value;
            rightHand.gameObject.SetActive(_hasRightHand.Value);
            leftHand.gameObject.SetActive(_hasLeftHand.Value);
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(PlayerNetwork))]
    public class PlayerNetworkEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var playerNetwork = (PlayerNetwork)target;
            GUILayout.Label(playerNetwork.IsOwner ? "Player is owner" : "Player is not owner");
            GUILayout.Label($"Player name: {playerNetwork.PlayerName}");
            GUILayout.Label($"Owner Id: {playerNetwork.OwnerClientId}");
            GUILayout.Label($"Player is ready: {playerNetwork.IsReady}");
            GUILayout.Label($"Placement Index: {playerNetwork.PlacementIndex}");
        }
    }
#endif
}