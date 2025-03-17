using System;
using System.Linq;
using CardGameVR.Cards.Groups;
using CardGameVR.Multiplayer;
using CardGameVR.Parties;
using CardGameVR.Players;
using CardGameVR.ScriptableObjects;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

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

        // Static properties

        public static int MaxPlayers => Instance.placements.Length;
        public static int MinPlayers => Instance.partyConfiguration.minPlayers;
        public static int MaxInHand => Instance.partyConfiguration.maxCardInHand;
        public static int InitialInHand => Instance.partyConfiguration.initialNumberInHand;
        public static int NumberOfLives => Instance.partyConfiguration.numberOfLives;



        // Instance properties

        public PartyConfiguration partyConfiguration;
        [SerializeField] public ArenaPlacement[] placements = Array.Empty<ArenaPlacement>();
        [SerializeField] public BoardGroup board;
        [SerializeField] public NetworkObject playerPrefab;

        // Unity Functions

        public void Start()
        {
            if (!Instance)
                Instance = this;
            else Destroy(gameObject);

            if (playerPrefab?.gameObject && NetworkManager.Singleton)
                NetworkManager.Singleton.AddNetworkPrefab(playerPrefab.gameObject);
            MultiplayerManager.OnClientJoined.AddListener(server_OnClientJoined);
        }

        public void OnDestroy()
        {
            if (Instance == this)
                Instance = null;

            if (playerPrefab?.gameObject && NetworkManager.Singleton)
                NetworkManager.Singleton.RemoveNetworkPrefab(playerPrefab.gameObject);
            MultiplayerManager.OnClientJoined.RemoveListener(server_OnClientJoined);
        }

        private void server_OnClientJoined(ClientJoinedArgs args)
        {
            if (!MultiplayerManager.IsServer()) return;
            
            Debug.Log($"Spawn player for client {args.ClientId}");
            var instance = Instantiate(Instance.playerPrefab.gameObject, transform);
            var obj = instance.GetComponent<NetworkObject>();
            obj.SpawnAsPlayerObject(args.ClientId, true);
        }
    }
}