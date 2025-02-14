using System;
using CardGameVR.API;
using CardGameVR.Teams;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace CardGameVR.Multiplayer
{
    public class MultiplayerManager : NetworkBehaviour
    {
        public static MultiplayerManager instance { get; private set; }

        [SerializeField] private MultiplayerConfig config;
        public TryToConnectEvent OnTryToConnect = new();

        public ClientDisconnectEvent OnClientDisconnect = new();
        public DisconnectEvent OnDisconnect = new();
        public static readonly ClientJoinedEvent OnClientJoined = new();
        public static readonly ClientLeftEvent OnClientLeft = new();

        public int MinPlayerCount => config.GetMinPlayerCount();
        public int MaxPlayerCount => config.GetMaxPlayerCount();
        public string PlayerName => BaseAPI.GetDisplayName();
        public string PlayerID => BaseAPI.GetId();
        
        public bool IsPlayerIsLocal(ulong clientId) => clientId == NetworkManager.Singleton.LocalClientId;

        public NetworkList<PlayerData> PlayerData;
        public static readonly PlayerDataChangedEvent OnPlayerDataChanged = new();

        private void Awake()
        {
            instance = this;
            PlayerData = new NetworkList<PlayerData>();
            PlayerData.OnListChanged += PlayerData_OnListChanged;
        }

        private void PlayerData_OnListChanged(NetworkListEvent<PlayerData> changeEvent)
            => OnPlayerDataChanged.Invoke(new PlayerDataChangedArgs { Manager = this, ChangeEvent = changeEvent });

        public void StartHost()
        {
            NetworkManager.Singleton.OnClientStarted += () => Debug.Log("ClientStarted");
            NetworkManager.Singleton.OnServerStopped += d => Debug.Log("ServerStopped");
            NetworkManager.Singleton.OnClientConnectedCallback += clientId => Debug.Log("ClientConnected");
            NetworkManager.Singleton.OnClientDisconnectCallback += clientId => Debug.Log("ClientDisconnect");
            NetworkManager.Singleton.OnConnectionEvent += (clientId, connectionState) => Debug.Log("ConnectionEvent");
            NetworkManager.Singleton.OnSessionOwnerPromoted += clientId => Debug.Log("SessionOwnerPromoted");
            NetworkManager.Singleton.OnTransportFailure += () => Debug.Log("TransportFailure");

            NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
            NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_Server_OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Server_OnClientDisconnectCallback;
            NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_Client_OnClientConnectedCallback;
            NetworkManager.Singleton.StartHost();
        }

        public void StartClient()
        {
            OnTryToConnect.Invoke(new TryToConnectArgs { Manager = this });
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Client_OnClientDisconnectCallback;
            NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_Client_OnClientConnectedCallback;
            NetworkManager.Singleton.StartClient();
        }

        private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request,
            NetworkManager.ConnectionApprovalResponse response)
        {
            response.Approved = NetworkManager.Singleton.ConnectedClientsList.Count < MaxPlayerCount;
        }

        private void NetworkManager_Server_OnClientConnectedCallback(ulong clientId)
        {
            byte freePlacement = 0;
            while (freePlacement < PlayerData.Count)
            {
                var found = false;
                foreach (var data in PlayerData)
                    if (data.Placement == freePlacement)
                    {
                        found = true;
                        break;
                    }

                if (!found) break;
                freePlacement++;
            }

            PlayerData.Add(new PlayerData
            {
                ClientId = clientId,
                Placement = freePlacement
            });
            NotifyPlayerJoinedClientRpc(clientId);
        }

        private void DisconnectClient(ulong clientId)
        {
            foreach (var data in PlayerData)
                if (data.ClientId == clientId)
                {
                    PlayerData.Remove(data);
                    break;
                }
        }

        private void NetworkManager_Server_OnClientDisconnectCallback(ulong clientId)
        {
            DisconnectClient(clientId);
            NotifyPlayerLeftClientRpc(clientId);
        }

        private void NetworkManager_Client_OnClientConnectedCallback(ulong obj)
        {
            SetPlayerIdServerRpc(new FixedString128Bytes(PlayerID));
            SetPlayerNameServerRpc(new FixedString128Bytes(PlayerName));
        }


        [ServerRpc(Delivery = RpcDelivery.Reliable, RequireOwnership = false)]
        private void SetPlayerIdServerRpc(FixedString128Bytes playerId, ServerRpcParams serverRpcParams = default)
        {
            var clientId = serverRpcParams.Receive.SenderClientId;
            var playerDataIndex = GetPlayerDataIndexFromClientId(clientId);
            var data = PlayerData[playerDataIndex];
            data.PlayerId = playerId;
            PlayerData[playerDataIndex] = data;
        }

        [ServerRpc(Delivery = RpcDelivery.Reliable, RequireOwnership = false)]
        private void SetPlayerNameServerRpc(FixedString128Bytes displayName, ServerRpcParams serverRpcParams = default)
        {
            var clientId = serverRpcParams.Receive.SenderClientId;
            var playerDataIndex = GetPlayerDataIndexFromClientId(clientId);
            var data = PlayerData[playerDataIndex];
            data.PlayerName = displayName;
            PlayerData[playerDataIndex] = data;
        }

        private int GetPlayerDataIndexFromClientId(ulong clientId)
        {
            for (var i = 0; i < PlayerData.Count; i++)
                if (PlayerData[i].ClientId == clientId)
                    return i;
            return -1;
        }

        private void NetworkManager_Client_OnClientDisconnectCallback(ulong clientId)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= NetworkManager_Client_OnClientDisconnectCallback;
            NetworkManager.Singleton.OnClientConnectedCallback -= NetworkManager_Client_OnClientConnectedCallback;
            OnDisconnect.Invoke(new DisconnectArgs { Manager = this });
        }

        [ClientRpc]
        private void NotifyPlayerJoinedClientRpc(ulong clientId)
        {
            Debug.Log($"Player {clientId} joined.");
            OnClientJoined.Invoke(new ClientJoinedArgs { Manager = this, ClientId = clientId });
        }

        [ClientRpc]
        private void NotifyPlayerLeftClientRpc(ulong clientId)
        {
            Debug.Log($"Player {clientId} left.");
            OnClientLeft.Invoke(new ClientLeftArgs { Manager = this, ClientId = clientId });
        }

        public bool TryGetPlayerData(ulong clientId, out PlayerData data)
        {
            foreach (var playerData in instance.PlayerData)
                if (playerData.ClientId == clientId)
                {
                    data = playerData;
                    return true;
                }

            data = default;
            return false;
        }
    }
}