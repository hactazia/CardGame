using System;
using System.Linq;
using CardGameVR.Cards.Groups;
using CardGameVR.Controllers;
using CardGameVR.Players;
using CardGameVR.ScriptableObjects;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace CardGameVR.Arenas
{
    /**
     * ArenaDescriptor is a singleton class that holds the current arena's information.
     * one arena by time and unique by scene.
     * It's make information for placing players and game board in the scene.
     */
    public class ArenaDescriptor : MonoBehaviour
    {
        public static ArenaDescriptor Instance;
        
        public PartyConfiguration partyConfiguration;

        [SerializeField] public ArenaPlacement[] placements = Array.Empty<ArenaPlacement>();
        public static int MaxPlayerCount => Instance.placements.Length;
        public static int MinPlayerCount => 2;

        [SerializeField] public GridCardGroup gameBoard;
        [SerializeField] public NetworkObject playerPrefab;


        public void Awake() => Instance = this;

        public void Start()
        {
            if (playerPrefab?.gameObject && NetworkManager.Singleton)
                NetworkManager.Singleton.AddNetworkPrefab(playerPrefab.gameObject);
            Multiplayer.MultiplayerManager.OnClientJoined.AddListener(OnClientJoined);
            Multiplayer.MultiplayerManager.OnClientLeft.AddListener(OnClientLeft);
        }

        public void OnDestroy()
        {
            if (Instance == this)
                Instance = null;

            if (playerPrefab?.gameObject && NetworkManager.Singleton)
                NetworkManager.Singleton.RemoveNetworkPrefab(playerPrefab.gameObject);
            Multiplayer.MultiplayerManager.OnClientJoined.RemoveListener(OnClientJoined);
            Multiplayer.MultiplayerManager.OnClientLeft.RemoveListener(OnClientLeft);
        }

        public ArenaPlacement GetFreePlacement()
            => placements.FirstOrDefault(placement => !placement.Player);

        private void OnClientJoined(Multiplayer.ClientJoinedArgs args)
            => OnClientJoinedAsync(args).Forget();

        private async UniTask OnClientJoinedAsync(Multiplayer.ClientJoinedArgs args)
        {
            await NetworkParty.WhenIsInstanced();

            var playerNetwork = PlayerNetwork.GetPlayer(args.ClientId);
            if (playerNetwork) return;

            if (Multiplayer.MultiplayerManager.IsServer())
            {
                Debug.Log($"Spawn player for client {args.ClientId}");
                var description = ArenaDescriptor.Instance;
                var playerInstance = Instantiate(description.playerPrefab.gameObject, transform);
                var player = playerInstance.GetComponent<NetworkObject>();
                player.SpawnAsPlayerObject(args.ClientId, true);
                playerNetwork = player.GetComponent<PlayerNetwork>();
            }

            playerNetwork = await PlayerNetwork.WhenPlayerSpawned(args.ClientId);

            var placement = GetFreePlacement();
            if (!placement) return;

            await placement.SetPlayer(playerNetwork);
        }

        private void OnClientLeft(Multiplayer.ClientLeftArgs args)
            => OnClientLeftAsync(args).Forget();

        private async UniTask OnClientLeftAsync(Multiplayer.ClientLeftArgs args)
        {
            await NetworkParty.WhenIsInstanced();

            GetPlacement(PlayerNetwork.GetPlayer(args.ClientId).PlacementIndex)
                ?.SetPlayer(null);
        }

        public ArenaPlacement GetPlacement(int value)
            => value < placements.Length ? placements[value] : null;
    }
}