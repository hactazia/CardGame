using System;
using System.Collections;
using System.Linq;
using CardGameVR.Arenas;
using CardGameVR.Cards;
using CardGameVR.Players;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

namespace CardGameVR
{
    public class NetworkParty : NetworkBehaviour
    {
        public static NetworkParty Instance { get; private set; }

        public void CheckAllPlayersReady()
        {
            if (ArenaDescriptor.Instance.partyConfiguration.minPlayers > PlayerNetwork.PlayerCount)
            {
                Debug.LogWarning("Not enough players");
                return;
            }

            if (ArenaDescriptor.Instance.placements
                .Where(placement => placement.Player)
                .Select(placement => PlayerNetwork.GetPlayer(placement.Player.OwnerClientId))
                .Any(playerNetwork => playerNetwork is null || !playerNetwork.IsReady))
            {
                Debug.LogWarning("Not all players are ready");
                return;
            }

            if (IsServer && !IsGameStarted)
                StartGame();
        }

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
                throw new System.Exception("Only the server can spawn the NetworkParty");
            if (Instance)
                Destroy(Instance.gameObject);
            Instance = null;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            Instance = this;
            _isGameStarted.OnValueChanged += client_OnGameStarted;
            _turnIndex.OnValueChanged += client_OnTurnIndexChanged;
        }

        private void client_OnGameStarted(bool previousValue, bool newValue)
        {
            if (previousValue == newValue) return;
            onGameStateChange.Invoke(newValue);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            if (Instance == this)
                Instance = null;
        }

        private readonly NetworkVariable<bool> _isGameStarted = new();

        public bool IsGameStarted
        {
            get => _isGameStarted.Value;
            set
            {
                if (IsServer)
                    _isGameStarted.Value = value;
                else SetIsGameStartedServerRpc(value);
            }
        }

        [ServerRpc]
        private void SetIsGameStartedServerRpc(bool isGameStarted)
            => _isGameStarted.Value = isGameStarted;

        public UnityEvent<bool> onGameStateChange = new();

        private readonly NetworkVariable<int> _turnIndex = new();
        public UnityEvent<int> onTurnIndexChanged = new();

        public int TurnIndex
        {
            get => _turnIndex.Value;
            set
            {
                if (IsServer)
                    _turnIndex.Value = value;
                else SetTurnIndexServerRpc(value);
            }
        }

        private void client_OnTurnIndexChanged(int previousValue, int newValue)
        {
            if (previousValue == newValue) return;
            onTurnIndexChanged.Invoke(newValue);
        }

        [ServerRpc]
        private void SetTurnIndexServerRpc(int turnIndex)
            => _turnIndex.Value = turnIndex;

        public void StartGame()
        {
            if (!Multiplayer.MultiplayerManager.IsServer())
                throw new System.Exception("Only the server can start the game");
            IsGameStarted = true;
            TurnIndex = 0;
            foreach (var player in PlayerNetwork.Players)
            {
                player.IsReady = true;
                var lives = new float[ArenaDescriptor.Instance.partyConfiguration.numberOfLives];
                for (var i = 0; i < lives.Length; i++)
                    lives[i] = 0;
                player.Lives = lives;
                player.ClearCards();
                DrawCards(player).Forget();
            }

            Debug.Log("Starting game");
        }

        private async UniTask DrawCards(PlayerNetwork player)
        {
            for (var i = 0;
                 i < ArenaDescriptor.Instance.partyConfiguration.initialNumberInHand &&
                 i < ArenaDescriptor.Instance.partyConfiguration.maxCardInHand;
                 i++)
            {
                if (i != 0)
                    await UniTask.Delay(500);
                player.DrawCard();
            }
        }

        public static async UniTask WhenIsInstanced()
        {
            while (!Instance)
                await UniTask.Yield();
        }

        public ICard[] GetBoardCards()
            => Array.Empty<ICard>();
    }
}