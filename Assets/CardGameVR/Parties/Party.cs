using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace CardGameVR.Parties
{
    public class Party : NetworkBehaviour
    {
        public static Party instance { get; private set; }
        [SerializeField] private PartyConfig config;

        private void Awake()
        {
            instance = this;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (!IsServer) return;

            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }


        private void SceneManager_OnLoadEventCompleted(string sceneName,
            UnityEngine.SceneManagement.LoadSceneMode loadSceneMode,
            List<ulong> clientsCompleted,
            List<ulong> clientsTimedOut)
        {
            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                var player = Instantiate(config.GetPlayerPrefab());
                player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
            }
            // GameBoard.Instance.Initialize();
            //PassPriority(0);
        }
    }
}