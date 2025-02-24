using System.Collections.Generic;
using System.Linq;
using CardGameVR.API;
using CardGameVR.Controllers;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace CardGameVR.Players
{
    public class PlayerNetwork : NetworkBehaviour
    {
        private static List<PlayerNetwork> Players { get; } = new();
        public static PlayerNetwork GetPlayer(ulong clientId) 
            => Players.FirstOrDefault(player => player.OwnerClientId == clientId);

        public Transform head;
        public Transform leftHand;
        public Transform rightHand;

        public UnityEvent<string> onPlayerNameChanged = new();
        public UnityEvent<string> onPlayerIdChanged = new();

        public void Awake()
        {
            Players.Add(this);
            _playerName.OnValueChanged += (_, newValue) => onPlayerNameChanged.Invoke(newValue.ToString());
            _playerId.OnValueChanged += (_, newValue) => onPlayerIdChanged.Invoke(newValue.ToString());
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

            Debug.Log($"PlayerNetwork has been spawned on the network with name: {_playerName.Value}");
        }

        [ServerRpc]
        private void SetPlayerNameServerRpc(FixedString128Bytes playerName)
            => _playerName.Value = playerName;

        [ServerRpc]
        private void SetPlayerIdServerRpc(FixedString128Bytes id)
            => _playerName.Value = id;


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
}