using CardGameVR.Arenas;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CardGameVR.Multiplayer
{
    public static class MultiplayerManager
    {
        public static readonly TryToConnectEvent OnTryToConnect = new();
        public static readonly DisconnectEvent OnDisconnect = new();
        public static readonly ConnectEvent OnConnect = new();
        public static readonly ClientJoinedEvent OnClientJoined = new();
        public static readonly ClientLeftEvent OnClientLeft = new();

        public static bool IsPlayerIsLocal(ulong clientId) => clientId == NetworkManager.Singleton.LocalClientId;
        public static bool IsServer() => NetworkManager.Singleton.IsServer;

        public static async UniTask StartHost()
        {
            NetworkManager.Singleton.OnClientStarted += () => Debug.Log("ClientStarted");
            NetworkManager.Singleton.OnServerStopped += OnServerStopped;
            NetworkManager.Singleton.OnClientConnectedCallback += _ => Debug.Log("ClientConnected");
            NetworkManager.Singleton.OnClientDisconnectCallback += _ => Debug.Log("ClientDisconnect");
            NetworkManager.Singleton.OnConnectionEvent += (_, _) => Debug.Log("ConnectionEvent");
            NetworkManager.Singleton.OnSessionOwnerPromoted += _ => Debug.Log("SessionOwnerPromoted");
            NetworkManager.Singleton.OnTransportFailure += () => Debug.Log("TransportFailure");
            NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
            NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_Server_OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Server_OnClientDisconnectCallback;
            NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_Client_OnClientConnectedCallback;
            NetworkManager.Singleton.StartHost();
            await NetworkParty.Spawn();
        }

        private static void OnServerStopped(bool a)
        {
            Debug.Log("Server stopped!");
            NetworkParty.Destroy();
        }

        public static void StartClient()
        {
            OnTryToConnect.Invoke(new TryToConnectArgs());
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Client_OnClientDisconnectCallback;
            NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_Client_OnClientConnectedCallback;
            NetworkManager.Singleton.StartClient();
        }

        private static void NetworkManager_ConnectionApprovalCallback(
            NetworkManager.ConnectionApprovalRequest request,
            NetworkManager.ConnectionApprovalResponse response
        )
        {
            response.Approved = NetworkManager.Singleton.ConnectedClientsList.Count < ArenaDescriptor.MaxPlayerCount;
        }

        private static void NetworkManager_Server_OnClientConnectedCallback(ulong clientId)
        {
            NotifyPlayerJoinedClientRpc(clientId);
        }

        private static void NetworkManager_Server_OnClientDisconnectCallback(ulong clientId)
        {
            NotifyPlayerLeftClientRpc(clientId);
        }

        private static void NetworkManager_Client_OnClientConnectedCallback(ulong obj)
        {
            Debug.Log("I'm connected!");
            OnConnect.Invoke(new ConnectArgs());
        }


        private static void NetworkManager_Client_OnClientDisconnectCallback(ulong clientId)
        {
            Debug.Log("I'm disconnected!");
            NetworkManager.Singleton.OnClientDisconnectCallback -= NetworkManager_Client_OnClientDisconnectCallback;
            NetworkManager.Singleton.OnClientConnectedCallback -= NetworkManager_Client_OnClientConnectedCallback;
            OnDisconnect.Invoke(new DisconnectArgs());
        }

        [ClientRpc]
        private static void NotifyPlayerJoinedClientRpc(ulong clientId)
        {
            Debug.Log($"Player {clientId} joined.");
            OnClientJoined.Invoke(new ClientJoinedArgs { ClientId = clientId });
        }

        [ClientRpc]
        private static void NotifyPlayerLeftClientRpc(ulong clientId)
        {
            Debug.Log($"Player {clientId} left.");
            OnClientLeft.Invoke(new ClientLeftArgs { ClientId = clientId });
        }
    }
}