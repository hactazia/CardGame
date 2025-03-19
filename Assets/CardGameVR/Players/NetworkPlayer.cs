using System.Collections.Generic;
using System.Linq;
using CardGameVR.API;
using CardGameVR.Arenas;
using CardGameVR.Cards;
using CardGameVR.Cards.Slots;
using CardGameVR.Controllers;
using CardGameVR.Parties;
using CardGameVR.Utils;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace CardGameVR.Players
{
    public class NetworkPlayer : NetworkBehaviour
    {
        public static List<NetworkPlayer> Players { get; } = new();

        // Static Functions

        public static NetworkPlayer GetPlayer(ulong clientId)
            => Players.FirstOrDefault(player => player.OwnerClientId == clientId);

        public static NetworkPlayer LocalPlayer
            => GetPlayer(NetworkManager.Singleton.LocalClientId);


        // Network Variables

        private readonly NetworkVariable<bool> _ready = new();
        private readonly NetworkVariable<int> _placement = new(-1);
        private readonly NetworkVariable<FixedString128Bytes> _name = new();
        private readonly NetworkList<HandCard> _hand = new();
        private readonly NetworkList<float> _lives = new();
        private readonly NetworkVariable<bool> _isAlive = new(true);
        private readonly NetworkVariable<int> _boosts = new(0);

        private readonly NetworkVariable<Vector3> _headPosition = new();
        private readonly NetworkVariable<Quaternion> _headRotation = new();
        private readonly NetworkVariable<Vector3> _leftHandPosition = new();
        private readonly NetworkVariable<Quaternion> _leftHandRotation = new();
        private readonly NetworkVariable<Vector3> _rightHandPosition = new();
        private readonly NetworkVariable<Quaternion> _rightHandRotation = new();
        private readonly NetworkVariable<bool> _hasRightHand = new();
        private readonly NetworkVariable<bool> _hasLeftHand = new();

        public static readonly UnityEvent<NetworkPlayer> OnPlayerJoined = new();
        public static readonly UnityEvent<NetworkPlayer> OnPlayerLeft = new();

        public static readonly UnityEvent<NetworkPlayer, bool> OnReady = new();
        public static readonly UnityEvent<NetworkPlayer, int> OnPlacement = new();
        public static readonly UnityEvent<NetworkPlayer, string> OnName = new();
        public static readonly UnityEvent<NetworkPlayer, float[]> OnLives = new();
        public static readonly UnityEvent<NetworkPlayer, bool> OnAlive = new();
        public static readonly UnityEvent<NetworkPlayer, int> OnBoosts = new();
        public static readonly UnityEvent<NetworkPlayer, HandCard> OnDrawHand = new();
        public static readonly UnityEvent<NetworkPlayer> OnClearHand = new();
        public static readonly UnityEvent<NetworkPlayer, HandCard> OnRemoveHand = new();


        public Transform head;
        public Transform leftHand;
        public Transform rightHand;


        // Network Functions

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsOwner)
                Name = BaseAPI.GetDisplayName();
            Players.Add(this);
            _name.OnValueChanged += client_OnNameChanged;
            _ready.OnValueChanged += client_OnReadyChanged;
            _placement.OnValueChanged += client_OnPlacementChanged;
            _isAlive.OnValueChanged += client_OnAliveChanged;
            _lives.OnListChanged += client_OnLivesChanged;
            _hand.OnListChanged += client_OnHandChanged;

            OnPlayerJoined.Invoke(this);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            Players.Remove(this);
            _name.OnValueChanged -= client_OnNameChanged;
            _ready.OnValueChanged -= client_OnReadyChanged;
            _placement.OnValueChanged -= client_OnPlacementChanged;
            _isAlive.OnValueChanged -= client_OnAliveChanged;
            _lives.OnListChanged -= client_OnLivesChanged;
            _hand.OnListChanged -= client_OnHandChanged;
            OnPlayerLeft.Invoke(this);
        }

        // Utility Functions

        public bool IsMyTurn
            => NetworkParty.Instance
               && NetworkParty.Instance.IsStarted
               && NetworkParty.Instance.Turn == OwnerClientId;

        // Implementation of Boosts


        public void BoostCard(int getId) 
            => NetworkParty.Instance.BoostCard(this, getId);

        public int Boosts
        {
            get => _boosts.Value;
            set => SetBoosts(value);
        }

        public void SetBoosts(int boosts)
        {
            if (IsServer) SetBoostsServer(boosts);
            else SetBoostsServerRpc(boosts);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SetBoostsServerRpc(int boosts)
            => SetBoostsServer(boosts);

        private void SetBoostsServer(int boosts)
            => _boosts.Value = boosts;

        // Implementation of Place Card on the board

        public void PlaceCard(ICard hand, CardSlot board)
            => NetworkParty.Instance.PlaceCard(this, hand, board);

        // Implementation of Move Card on the board

        public void MoveCard(CardSlot slotFrom, CardSlot slotTo)
            => NetworkParty.Instance.MoveCard(this, slotFrom, slotTo);

        // Implementation of Draw Card

        public void DrawCard()
        {
            if (IsServer) DrawCardServer();
            else DrawCardServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        private void DrawCardServerRpc() => DrawCardServer();

        private void DrawCardServer()
        {
            var draw = CardTypeManager.DrawType();
            if (string.IsNullOrEmpty(draw))
            {
                Debug.LogError("No card to draw");
                return;
            }

            var index = CardTypeManager.GetNextId();

            _hand.Add(new HandCard
            {
                CardType = new FixedString32Bytes(draw),
                IsVisibleForLocalPlayer = true,
                IsVisibleForOtherPlayers = false,
                Id = index
            });
        }


        // Implementation of Clear Hand

        public void ClearHand()
        {
            if (IsServer) ClearHandServer();
            else ClearHandServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        private void ClearHandServerRpc()
            => ClearHandServer();

        private void ClearHandServer()
            => _hand.Clear();

        // Implementation of PlayerName

        public string Name
        {
            get => _name.Value.ToString();
            set => SetName(value);
        }

        public void SetName(string name)
        {
            if (IsServer) SetNameServer(name);
            else SetNameServerRpc(name);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SetNameServerRpc(string name)
            => SetNameServer(name);

        private void SetNameServer(string name)
            => _name.Value = new FixedString128Bytes(name);

        // Implementation of IsAlive

        public bool IsAlive
        {
            get => _isAlive.Value;
            set => SetAlive(value);
        }

        public void SetAlive(bool alive)
        {
            if (IsServer) SetAliveServer(alive);
            else SetAliveServerRpc(alive);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SetAliveServerRpc(bool alive)
            => SetAliveServer(alive);

        private void SetAliveServer(bool alive)
            => _isAlive.Value = alive;

        // Implementation of Ready

        public bool Ready
        {
            get => _ready.Value;
            set => SetReady(value);
        }

        public void SetReady(bool ready)
        {
            if (IsServer) SetReadyServer(ready);
            else SetReadyServerRpc(ready);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SetReadyServerRpc(bool ready)
            => SetReadyServer(ready);

        private void SetReadyServer(bool ready)
            => _ready.Value = ready;

        // Implementation of Lives

        public float[] Lives
        {
            get => NetworkListExtension.ToArray(_lives);
            set => SetLives(value);
        }

        public void SetLives(float[] lives)
        {
            if (IsServer) SetLivesServer(lives);
            else SetLivesServerRpc(lives);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SetLivesServerRpc(float[] lives)
            => SetLivesServer(lives);

        private void SetLivesServer(float[] lives)
        {
            _lives.Clear();
            foreach (var life in lives)
                _lives.Add(life);
            foreach (var live in _lives)
                if (ArenaDescriptor.HealthRange.x > live || live > ArenaDescriptor.HealthRange.y)
                    IsAlive = false;
        }

        // Implementation of Hand

        public HandCard[] Hand => NetworkListExtension.ToArray(_hand);

        public HandCard GetHandByIndex(int index)
            => NetworkListExtension
                .ToList(_hand)
                .Find(x => x.Id == index);

        // Implementation of Placement

        public int Placement
        {
            get => _placement.Value;
            set => SetPlacement(value);
        }

        public void SetPlacement(int placement)
        {
            if (IsServer) SetPlacementServer(placement);
            else SetPlacementServerRpc(placement);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SetPlacementServerRpc(int placement)
            => SetPlacementServer(placement);

        private void SetPlacementServer(int placement)
            => _placement.Value = placement;

        // Event Listeners

        private void client_OnHandChanged(NetworkListEvent<HandCard> changeEvent)
        {
            switch (changeEvent.Type)
            {
                case NetworkListEvent<HandCard>.EventType.Add:
                    OnDrawHand.Invoke(this, changeEvent.Value);
                    break;
                case NetworkListEvent<HandCard>.EventType.Clear:
                    OnClearHand.Invoke(this);
                    break;
                case NetworkListEvent<HandCard>.EventType.RemoveAt:
                    OnRemoveHand.Invoke(this, changeEvent.Value);
                    break;
                default:
                    Debug.LogError($"Unknown network event type for hand: {changeEvent.Type}");
                    break;
            }
        }

        private void client_OnNameChanged(FixedString128Bytes oldValue, FixedString128Bytes newValue)
            => OnName.Invoke(this, newValue.ToString());

        private void client_OnReadyChanged(bool oldValue, bool newValue)
            => OnReady.Invoke(this, newValue);

        private void client_OnPlacementChanged(int oldValue, int newValue)
            => OnPlacement.Invoke(this, newValue);

        private void client_OnLivesChanged(NetworkListEvent<float> changeEvent)
            => OnLives.Invoke(this, Lives);

        private void client_OnAliveChanged(bool oldValue, bool newValue)
            => OnAlive.Invoke(this, newValue);


        public override void OnDestroy()
        {
            base.OnDestroy();
            Players.Remove(this);
        }

        // Implementation of Remove Hand At

        public void RemoveHandAt(int index)
        {
            if (IsServer) RemoveHandAtServer(index);
            else RemoveHandAtServerRpc(index);
        }

        [ServerRpc(RequireOwnership = false)]
        private void RemoveHandAtServerRpc(int index)
            => RemoveHandAtServer(index);

        private void RemoveHandAtServer(int index)
            => _hand.RemoveAt(index);

        // Player Transform

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

        [ServerRpc(RequireOwnership = false)]
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
#if UNITY_EDITOR
        [UnityEditor.CustomEditor(typeof(NetworkPlayer))]
        public class PlayerNetworkEditor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
                var playerNetwork = (NetworkPlayer)target;
                UnityEditor.EditorGUILayout.LabelField(
                    playerNetwork.IsOwner ? "Player is owner" : "Player is not owner");
                UnityEditor.EditorGUILayout.LabelField($"Player name: {playerNetwork.Name}");
                UnityEditor.EditorGUILayout.LabelField($"Owner Id: {playerNetwork.OwnerClientId}");
                UnityEditor.EditorGUILayout.LabelField($"Player is ready: {playerNetwork.Ready}");
                UnityEditor.EditorGUILayout.LabelField($"Placement Index: {playerNetwork.Placement}");
                UnityEditor.EditorGUILayout.Space();

                if (playerNetwork.IsOwner)
                {
                    if (UnityEditor.EditorGUILayout.LinkButton("Draw card"))
                        playerNetwork.DrawCard();
                    if (UnityEditor.EditorGUILayout.LinkButton(playerNetwork.Ready ? "Unready" : "Ready"))
                        playerNetwork.Ready = !playerNetwork.Ready;
                }

                UnityEditor.EditorGUILayout.Space();
                UnityEditor.EditorGUILayout.LabelField($"Cards in hand: {playerNetwork.Hand.Length}");
                foreach (var card in playerNetwork.Hand)
                    UnityEditor.EditorGUILayout.LabelField($"(${card.CardType}) {card.Id}");

                UnityEditor.EditorGUILayout.Space();

                // life
                UnityEditor.EditorGUILayout.LabelField("Life");
                var lives = playerNetwork.Lives;
                for (var i = 0; i < lives.Length; i++)
                {
                    var life = playerNetwork._lives[i];
                    UnityEditor.EditorGUILayout.BeginHorizontal();
                    UnityEditor.EditorGUILayout.LabelField($"Life {i}");
                    life = UnityEditor.EditorGUILayout.Slider(life, -1, 1);
                    UnityEditor.EditorGUILayout.EndHorizontal();
                    lives[i] = life;
                }

                lives = playerNetwork.Lives;
            }
        }
#endif
    }
}