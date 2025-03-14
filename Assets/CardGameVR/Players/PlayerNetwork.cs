using System;
using System.Collections.Generic;
using System.Linq;
using CardGameVR.API;
using CardGameVR.Arenas;
using CardGameVR.Cards;
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

        public ICard[] GetHandCards()
        {
            var placement = ArenaDescriptor.Instance.GetPlacement(PlacementIndex);
            return !placement
                ? Array.Empty<ICard>()
                : placement.handCardGroup.GetCards();
        }

        public Transform head;
        public Transform leftHand;
        public Transform rightHand;

        public UnityEvent<string> onPlayerNameChanged = new();
        public UnityEvent<string> onPlayerIdChanged = new();
        public UnityEvent<bool> onIsReadyChanged = new();
        public UnityEvent<int, int> onPlacementIndexChanged = new();

        public UnityEvent<PlayerHandCard> onPlayerCardAdded = new();
        public UnityEvent<PlayerHandCard> onPlayerCardRemoved = new();
        public UnityEvent<PlayerHandCard> onPlayerCardUpdated = new();
        public UnityEvent onPlayerCardCleared = new();

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
            _handCards.OnListChanged += OnPlayerCardChanged;
        }

        private void OnPlayerCardChanged(NetworkListEvent<PlayerHandCard> changeEvent)
        {
            switch (changeEvent.Type)
            {
                case NetworkListEvent<PlayerHandCard>.EventType.Add:
                    onPlayerCardAdded.Invoke(changeEvent.Value);
                    break;
                case NetworkListEvent<PlayerHandCard>.EventType.Remove:
                    onPlayerCardRemoved.Invoke(changeEvent.Value);
                    break;
                case NetworkListEvent<PlayerHandCard>.EventType.Insert:
                    onPlayerCardUpdated.Invoke(changeEvent.Value);
                    break;
                case NetworkListEvent<PlayerHandCard>.EventType.Clear:
                    onPlayerCardCleared.Invoke();
                    break;
            }
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
                ArenaDescriptor.Instance.GetPlacement(oldValue)?.SetPlayer(null, true);
            if (newValue != -1)
                ArenaDescriptor.Instance.GetPlacement(newValue)?.SetPlayer(this, true);
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
            rightHand.gameObject.SetActive(!IsLocalPlayer);
            leftHand.gameObject.SetActive(!IsLocalPlayer);

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
        private readonly NetworkVariable<int> _placementIndex = new(-1);
        private readonly NetworkList<float> _lives = new();

        private readonly NetworkList<PlayerHandCard> _handCards = new();

        public PlayerHandCard[] Hand
        {
            get
            {
                var cards = new PlayerHandCard[_handCards.Count];
                for (var i = 0; i < _handCards.Count; i++)
                    cards[i] = _handCards[i];
                return cards;
            }
        }

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

        public void DrawCard()
        {
            if (IsServer)
                AddCardToHand();
            else DrawCardServerRpc();
        }

        [ServerRpc]
        private void DrawCardServerRpc()
        {
            Debug.Log($"DrawCardServerRpc({OwnerClientId})");
            AddCardToHand();
        }

        private void AddCardToHand()
        {
            Debug.Log($"AddCardToHand({OwnerClientId})");
            var draw = CardTypeManager.DrawType();
            if (string.IsNullOrEmpty(draw))
            {
                Debug.LogError("No card to draw");
                return;
            }

            _handCards.Add(new PlayerHandCard
            {
                CardType = new FixedString32Bytes(draw),
                IsVisibleForLocalPlayer = true,
                IsVisibleForOtherPlayers = false,
                Id = CardTypeManager.GetNextId()
            });
        }

        public void ClearCards()
        {
            if (IsServer)
                _handCards.Clear();
            else ClearCardsServerRpc();
        }

        [ServerRpc]
        private void ClearCardsServerRpc()
            => _handCards.Clear();

#if UNITY_EDITOR
        [UnityEditor.CustomEditor(typeof(PlayerNetwork))]
        public class PlayerNetworkEditor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
                var playerNetwork = (PlayerNetwork)target;
                UnityEditor.EditorGUILayout.LabelField(
                    playerNetwork.IsOwner ? "Player is owner" : "Player is not owner");
                UnityEditor.EditorGUILayout.LabelField($"Player name: {playerNetwork.PlayerName}");
                UnityEditor.EditorGUILayout.LabelField($"Owner Id: {playerNetwork.OwnerClientId}");
                UnityEditor.EditorGUILayout.LabelField($"Player is ready: {playerNetwork.IsReady}");
                UnityEditor.EditorGUILayout.LabelField($"Placement Index: {playerNetwork.PlacementIndex}");
                UnityEditor.EditorGUILayout.Space();

                if (playerNetwork.IsOwner)
                {
                    if (UnityEditor.EditorGUILayout.LinkButton("Draw card"))
                        playerNetwork.DrawCard();
                    if (UnityEditor.EditorGUILayout.LinkButton(playerNetwork.IsReady ? "Unready" : "Ready"))
                        playerNetwork.IsReady = !playerNetwork.IsReady;
                }

                UnityEditor.EditorGUILayout.Space();
                UnityEditor.EditorGUILayout.LabelField($"Cards in hand: {playerNetwork._handCards.Count}");
                foreach (var card in playerNetwork._handCards)
                    UnityEditor.EditorGUILayout.LabelField($"(${card.CardType}) {card.Id}");
                UnityEditor.EditorGUILayout.Space();
                // life
                UnityEditor.EditorGUILayout.LabelField("Life");
                for (var i = 0; i < playerNetwork._lives.Count; i++)
                {
                    var life = playerNetwork._lives[i];
                    UnityEditor.EditorGUILayout.BeginHorizontal();
                    UnityEditor.EditorGUILayout.LabelField($"Life {i}");
                    life = UnityEditor.EditorGUILayout.Slider(life, -1, 1);
                    UnityEditor.EditorGUILayout.EndHorizontal();
                    playerNetwork._lives[i] = life;
                }
            }
        }
#endif
    }
}