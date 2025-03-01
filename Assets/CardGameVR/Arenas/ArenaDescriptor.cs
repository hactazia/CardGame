using System;
using CardGameVR.Cards.Groups;
using CardGameVR.Controllers;
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

        [SerializeField] public ArenaPlacement[] placements = Array.Empty<ArenaPlacement>();
        public static int MaxPlayerCount => Instance.placements.Length;

        [SerializeField] public GridCardGroup gameBoard;
        [SerializeField] public NetworkObject playerPrefab;

        internal static int GetPlayerIndex(ulong clientId)
        {
            for (var i = 0; i < Instance.placements.Length; i++)
                if (Instance.placements[i].clientId == clientId)
                    return i;
            return -1;
        }

        public void Awake() => Instance = this;

        public void Start()
        {
            if (playerPrefab?.gameObject && NetworkManager.Singleton)
                NetworkManager.Singleton.AddNetworkPrefab(playerPrefab.gameObject);
        }

        public void OnDestroy()
        {
            if (Instance == this)
                Instance = null;

            if (playerPrefab?.gameObject && NetworkManager.Singleton)
                NetworkManager.Singleton.RemoveNetworkPrefab(playerPrefab.gameObject);
        }
    }
}