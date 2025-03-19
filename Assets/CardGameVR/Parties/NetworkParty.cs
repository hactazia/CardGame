using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CardGameVR.Arenas;
using CardGameVR.Cards;
using CardGameVR.Cards.Slots;
using CardGameVR.Players;
using CardGameVR.Utils;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

namespace CardGameVR.Parties
{
    public class NetworkParty : NetworkBehaviour
    {
        public static NetworkParty Instance { get; private set; }

        private readonly NetworkList<GridCard> _board = new();
        private readonly NetworkVariable<bool> _state = new();
        private readonly NetworkVariable<ulong> _turn = new();

        public static readonly UnityEvent OnGameStarted = new();
        public static readonly UnityEvent OnGameEnd = new();
        public static readonly UnityEvent<bool> OnState = new();

        public static readonly UnityEvent<NetworkPlayer> OnTurn = new();
        public static readonly UnityEvent<NetworkPlayer, GridCard> OnBoardCardPlaced = new();
        public static readonly UnityEvent<NetworkPlayer, GridCard, int> OnBoardCardMoved = new();
        public static readonly UnityEvent<NetworkPlayer, int> OnBoardCardDestroyed = new();
        public static readonly UnityEvent OnBoardCleared = new();
        public static readonly UnityEvent<NetworkPlayer, GridCard, int, bool> OnBoardBoosted = new();

        // Network Functions

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _turn.OnValueChanged += client_OnTurn;
            _state.OnValueChanged += client_OnState;
            _board.OnListChanged += client_OnBoardChanged;
            Instance = this;
        }


        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            _turn.OnValueChanged -= client_OnTurn;
            _state.OnValueChanged -= client_OnState;
            _board.OnListChanged -= client_OnBoardChanged;
            if (Instance == this)
                Instance = null;
        }


        // Utility Functions
        public GridCard[] Board => NetworkListExtension.ToArray(_board);
        public bool IsStarted => Instance._state.Value;
        public ulong Turn => _turn.Value;

        public NetworkPlayer IsTurn()
            => NetworkPlayer.GetPlayer(_turn.Value);


        // Implementation of Place Card on the board


        public void PlaceCard(NetworkPlayer networkPlayer, ICard hand, CardSlot board)
        {
            if (IsServer) PlaceCardServer(networkPlayer.OwnerClientId, hand.GetId(), board.Index).Forget();
            else PlaceCardServerRpc(networkPlayer.OwnerClientId, hand.GetId(), board.Index);
        }

        [ServerRpc(RequireOwnership = false)]
        private void PlaceCardServerRpc(ulong clientId, int cardId, int boardPos)
            => PlaceCardServer(clientId, cardId, boardPos).Forget();

        private async UniTask PlaceCardServer(ulong clientId, int cardId, int boardPos)
        {
            var player = NetworkPlayer.GetPlayer(clientId);
            var handIndex = Array.FindIndex(player.Hand, c => c.Id == cardId);
            var boardIndex = Array.FindIndex(NetworkListExtension.ToArray(_board), c => c.Index == boardPos);
            var hand = player.Hand[handIndex];

            if (boardIndex == -1)
                _board.Add(new GridCard
                {
                    Index = boardPos,
                    CardType = hand.CardType,
                    OwnerId = player.OwnerClientId,
                    Id = hand.Id
                });
            else
                _board.Insert(boardIndex, new GridCard
                {
                    Index = boardPos,
                    CardType = hand.CardType,
                    OwnerId = player.OwnerClientId,
                    Id = hand.Id
                });


            BroadcastPlaceCardClientRpc(clientId, hand.Id);

            player.RemoveHandAt(handIndex);
        }

        [ClientRpc]
        private void BroadcastPlaceCardClientRpc(ulong clientId, int cardId)
            => BroadcastPlaceCardClientRpcAsync(clientId, cardId).Forget();


        private async UniTask BroadcastPlaceCardClientRpcAsync(ulong clientId, int cardId)
        {
            var index = -1;
            var board = NetworkListExtension.ToArray(_board);
            while (index == -1)
            {
                await UniTask.Yield();
                board = NetworkListExtension.ToArray(_board);
                index = Array.FindIndex(board, c => c.Id == cardId);
            }

            OnBoardCardPlaced.Invoke(NetworkPlayer.GetPlayer(clientId), board[index]);
        }

        // Implementation of Destroy Card on the board

        public void DestroyCard(NetworkPlayer networkPlayer, CardSlot slot)
        {
            if (IsServer) DestroyCardServer(networkPlayer.OwnerClientId, slot.Index);
            else DestroyCardServerRpc(networkPlayer.OwnerClientId, slot.Index);
        }

        [ServerRpc(RequireOwnership = false)]
        private void DestroyCardServerRpc(ulong clientId, int boardPos)
            => DestroyCardServer(clientId, boardPos);

        private void DestroyCardServer(ulong clientId, int boardPos)
        {
            var boardIndex = Array.FindIndex(NetworkListExtension.ToArray(_board), c => c.Index == boardPos);
            _board.RemoveAt(boardIndex);
            BroadcastDestroyCardClientRpc(clientId, boardPos);
        }

        [ClientRpc]
        private void BroadcastDestroyCardClientRpc(ulong clientId, int boardPos)
            => OnBoardCardDestroyed.Invoke(NetworkPlayer.GetPlayer(clientId), boardPos);

        // Implementation of Move Card on the board

        public void MoveCard(NetworkPlayer networkPlayer, CardSlot slotStart, CardSlot slotEnd)
        {
            if (IsServer) MoveCardServer(networkPlayer.OwnerClientId, slotStart.Index, slotEnd.Index);
            else MoveCardServerRpc(networkPlayer.OwnerClientId, slotStart.Index, slotEnd.Index);
        }

        [ServerRpc(RequireOwnership = false)]
        private void MoveCardServerRpc(ulong clientId, int oldPos, int newPos)
            => MoveCardServer(clientId, oldPos, newPos);

        private void MoveCardServer(ulong clientId, int oldPos, int newPos)
        {
            var player = NetworkPlayer.GetPlayer(clientId);
            var board = NetworkListExtension.ToList(_board);
            var oldIndex = board.FindIndex(card => card.Index == oldPos);
            var newIndex = board.FindIndex(card => card.Index == newPos);

            _board.Insert(oldIndex, new GridCard
            {
                Id = board[oldIndex].Id,
                CardType = board[oldIndex].CardType,
                Index = newPos,
                OwnerId = clientId
            });
            _board.RemoveAt(oldIndex + 1);

            BroadcastMoveCardClientRpc(clientId, board[oldIndex].Id, oldPos, newPos);
            if (newIndex != -1)
                _board.RemoveAt(newIndex);
        }

        [ClientRpc]
        private void BroadcastMoveCardClientRpc(ulong clientId, int cardId, int oldPos, int newPos)
            => BroadcastMoveCardClientRpcAsync(clientId, cardId, oldPos, newPos).Forget();


        private async UniTask BroadcastMoveCardClientRpcAsync(ulong clientId, int cardId, int oldPos, int newPos)
        {
            var index = -1;
            var board = NetworkListExtension.ToArray(_board);
            while (index == -1)
            {
                await UniTask.Yield();
                board = NetworkListExtension.ToArray(_board);
                index = Array.FindIndex(board, c => c.Id == cardId && c.Index == newPos);
            }

            OnBoardCardMoved.Invoke(NetworkPlayer.GetPlayer(clientId), board[index], oldPos);
        }

        // Implementation of Game Start


        public void StartGame()
        {
            if (Instance.IsServer)
                Instance.StartGameServer();
            else Instance.StartGameServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        private void StartGameServerRpc()
            => StartGameServer();

        private void StartGameServer()
        {
            var players = NetworkPlayer.Players;
            foreach (var player in players)
            {
                List<float> list = new();
                for (var i = 0; i < ArenaDescriptor.NumberOfLives; i++)
                    list.Add(ArenaDescriptor.InitialHealth);
                player.IsAlive = true;
                player.Lives = list.ToArray();
            }

            _turn.Value = players[UnityEngine.Random.Range(0, players.Count)].OwnerClientId;
            _state.Value = true;
        }

        // Implementation of Next Turn

        public void NextTurn()
        {
            if (IsServer) NextTurnServer();
            else NextTurnServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        private void NextTurnServerRpc()
            => NextTurnServer();

        private void NextTurnServer()
        {
            var old = _turn.Value;
            var index = NetworkPlayer.Players.FindIndex(player => player.OwnerClientId == _turn.Value);
            var player = NetworkPlayer.Players[index];
            do
            {
                index = (index + 1) % NetworkPlayer.Players.Count;
                player = NetworkPlayer.Players[index];
                if (old != player.OwnerClientId) continue;
                _state.Value = false;
                return;
            } while (!player.IsAlive);

            if (player.Boosts < ArenaDescriptor.MaxBoosts &&
                UnityEngine.Random.Range(0, 100) / 100f < ArenaDescriptor.BoostRarity)
                player.Boosts++;

            _turn.Value = player.OwnerClientId;
            Debug.Log($"Turn: {_turn.Value} (was {old})");
            client_OnTurn(old, _turn.Value);
        }

        // Events

        private void client_OnTurn(ulong oldValue, ulong newValue)
        {
            Debug.Log("OnTurn");
            OnTurn.Invoke(NetworkPlayer.GetPlayer(newValue));
        }

        private void client_OnState(bool oldValue, bool newValue)
        {
            (newValue ? OnGameStarted : OnGameEnd).Invoke();
            OnState.Invoke(newValue);
        }

        private void client_OnBoardChanged(NetworkListEvent<GridCard> changeEvent)
        {
            switch (changeEvent.Type)
            {
                case NetworkListEvent<GridCard>.EventType.Clear:
                    OnBoardCleared.Invoke();
                    break;
                default:
                    Debug.LogError($"Unknown event type (board): {changeEvent.Type}");
                    break;
            }
        }
        // Class Functions

        public static async UniTask<NetworkParty> Spawn()
        {
            if (!Multiplayer.MultiplayerManager.IsServer())
                throw new System.Exception("Only the server can spawn the NetworkParty");
            var asset = await Addressables.LoadAssetAsync<GameObject>("NetworkParty");
            var go = Instantiate(asset);
            go.name = $"[{nameof(NetworkParty)}]";
            DontDestroyOnLoad(go);
            var party = go.GetComponent<NetworkParty>();
            var networkObject = go.GetComponent<NetworkObject>();
            if (networkObject && !networkObject.IsSpawned)
                networkObject.Spawn();
            Instance = party;
            return party;
        }

        public static void Destroy()
        {
            if (!Multiplayer.MultiplayerManager.IsServer())
                throw new Exception("Only the server can destroy the NetworkParty");
            if (Instance)
                Destroy(Instance.gameObject);
            Instance = null;
        }

        public void ClearBoard()
        {
            if (IsServer) ClearBoardServer();
            else ClearBoardServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        private void ClearBoardServerRpc()
            => ClearBoardServer();

        private void ClearBoardServer()
            => _board.Clear();

        // Implementation of Boost Card

        public void BoostCard(NetworkPlayer player, int getId)
        {
            if (IsServer) BoostCardServer(player.OwnerClientId, getId);
            else BoostCardServerRpc(player.OwnerClientId, getId);
        }

        [ServerRpc(RequireOwnership = false)]
        private void BoostCardServerRpc(ulong clientId, int getId)
            => BoostCardServer(clientId, getId);

        private void BoostCardServer(ulong clientId, int getId)
        {
            var player = NetworkPlayer.GetPlayer(clientId);
            if (player.Boosts <= 0) return;
            var index = Array.FindIndex(NetworkListExtension.ToArray(_board), c => c.Id == getId);
            if (index == -1) return;
            var card = _board[index];
            if (card.IsBoosted) return;
            card.IsBoosted = true;
            _board.Insert(index, card);
            _board.RemoveAt(index + 1);
            player.Boosts = Mathf.Max(0, player.Boosts - 1);

            BroadcastBoostCardClientRpc(clientId, getId, player.Boosts, true);
        }

        [ClientRpc]
        private void BroadcastBoostCardClientRpc(ulong clientId, int getId, int boosts, bool isBoosted)
            => BroadcastBoostCardClientRpcAsync(clientId, getId, boosts, isBoosted).Forget();

        private async UniTask BroadcastBoostCardClientRpcAsync(ulong clientId, int getId, int boosts, bool isBoosted)
        {
            var index = -1;
            var board = NetworkListExtension.ToArray(_board);
            while (index == -1)
            {
                await UniTask.Yield();
                board = NetworkListExtension.ToArray(_board);
                index = Array.FindIndex(board, c => c.Id == getId && c.IsBoosted == isBoosted);
            }

            OnBoardBoosted.Invoke(NetworkPlayer.GetPlayer(clientId), board[index], boosts, isBoosted);
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(NetworkParty))]
    public class NetworkPartyEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var party = (NetworkParty)target;
            UnityEditor.EditorGUILayout.LabelField("Is Game Started", party.IsStarted.ToString());
            UnityEditor.EditorGUILayout.LabelField("Turn Player", party.Turn.ToString());
            UnityEditor.EditorGUILayout.LabelField("Players", NetworkPlayer.Players.Count.ToString());
            foreach (var player in NetworkPlayer.Players)
                UnityEditor.EditorGUILayout.LabelField($" - {player.OwnerClientId}");
            UnityEditor.EditorGUILayout.LabelField("Board Cards", party.Board.Length.ToString());
            foreach (var card in party.Board)
                UnityEditor.EditorGUILayout.LabelField($" - {card.CardType} at {card.Index}");
        }
    }
#endif
}