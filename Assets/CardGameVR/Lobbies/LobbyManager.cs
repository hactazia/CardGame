using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CardGameVR.Multiplayer;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace CardGameVR.Lobbies
{
    public class LobbyManager : MonoBehaviour
    {
        public static LobbyManager instance { get; private set; }

        private const float HeartbeatDelay = 15f;
        public const float RefreshDelay = 15f;
        private const string KeyRelayJoinCode = "c";
        private const string KeyProtocolVersion = "v";
        private const string KeyHostBuildVersion = "b";

        private static readonly QueryLobbiesOptions QueryLobbiesOptions = new()
        {
            Filters = new List<QueryFilter>
            {
                new(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
            }
        };

        public bool autoRefresh = false;
        private Lobby _currentLobby;
        private DateTime _lastHeartbeat;
        public DateTime LastRefresh;

        public static readonly LobbyExceptionEvent OnCreateLobbyFailed = new();
        public static readonly LobbyExceptionEvent OnRefusedToJoinLobby = new();
        public static readonly LobbyExceptionEvent OnJoinLobbyByIdFailed = new();
        public static readonly RefreshLobbiesEvent OnRefreshLobbies = new();
        public static readonly CreatingLobbyEvent OnCreatingLobby = new();
        public static readonly JoiningLobbyEvent OnJoiningLobby = new();

        private void Awake()
        {
            instance = this;

            InitializeAuthenticationServices().Forget();

            _lastHeartbeat = DateTime.MinValue;
            LastRefresh = DateTime.MinValue;
            _currentLobby = null;

            AuthenticationService.Instance.SignedIn += AuthenticationService_SignedIn;
        }

        private async UniTask InitializeAuthenticationServices()
        {
            if (UnityServices.State == ServicesInitializationState.Initialized) return;
            var options = new InitializationOptions();
            options.SetProfile(UnityEngine.Random.Range(0, 10000).ToString());
            await UnityServices.InitializeAsync(options);
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        private void AuthenticationService_SignedIn()
            => RefreshLobbies().Forget();

        private void Update()
        {
            HandleLobbyHeartBeat();
            HandleLobbyRefresh();
        }

        private bool LobbyExists(Lobby lobby) => lobby != null;

        private bool IsLobbyHost(Lobby lobby)
        {
            if (!LobbyExists(lobby)) return false;
            return lobby.HostId == AuthenticationService.Instance.PlayerId;
        }


        private void HandleLobbyHeartBeat()
        {
            if (!LobbyExists(_currentLobby)) return;
            if (_lastHeartbeat == DateTime.MaxValue) return;
            if (DateTime.Now - _lastHeartbeat <= TimeSpan.FromSeconds(HeartbeatDelay)) return;
            _lastHeartbeat = DateTime.Now;
            LobbyHeartBeat().Forget();
        }

        private async UniTask LobbyHeartBeat()
        {
            Debug.Log("Sending heartbeat");
            _lastHeartbeat = DateTime.MaxValue;
            try
            {
                await LobbyService.Instance.SendHeartbeatPingAsync(_currentLobby.Id);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogException(e);
            }

            _lastHeartbeat = DateTime.Now;
        }


        private void HandleLobbyRefresh()
        {
            if (!autoRefresh) return;
            if (!AuthenticationService.Instance.IsSignedIn) return;
            if (LastRefresh == DateTime.MaxValue) return;
            if (DateTime.Now - LastRefresh <= TimeSpan.FromSeconds(RefreshDelay)) return;
            LastRefresh = DateTime.Now;
            RefreshLobbies().Forget();
        }


        private async UniTask RefreshLobbies()
        {
            LastRefresh = DateTime.MaxValue;
            try
            {
                var res = await LobbyService.Instance.QueryLobbiesAsync(QueryLobbiesOptions);
                OnRefreshLobbies.Invoke(new RefreshLobbiesArgs { Lobbies = res.Results.ToArray(), Manager = this });
            }
            catch (LobbyServiceException e)
            {
                Debug.LogException(e);
            }

            LastRefresh = DateTime.Now;
        }

        private async UniTask<Allocation> AllocateRelay()
        {
            try
            {
                return await RelayService.Instance
                    .CreateAllocationAsync(MultiplayerManager.instance.maxPlayerCount - 1);
            }
            catch (RelayServiceException e)
            {
                Debug.LogException(e);
                return default;
            }
        }

        private async UniTask<string> GetRelayJoinCode(Allocation allocation)
        {
            try
            {
                return await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            }
            catch (RelayServiceException e)
            {
                Debug.LogException(e);
                return default;
            }
        }


        public async UniTask<Lobby> CreateLobby(string lobbyName, bool isPrivate)
        {
            try
            {
                OnCreatingLobby.Invoke(new CreatingLobbyArgs
                    { Manager = this, Status = CreatingLobbyStatus.RelayAllocation });
                var allocation = await AllocateRelay();
                OnCreatingLobby.Invoke(new CreatingLobbyArgs
                    { Manager = this, Status = CreatingLobbyStatus.RelayJoinCode });
                var relayJoinCode = await GetRelayJoinCode(allocation);
                var protocolVersion = NetworkManager.Singleton.NetworkConfig.ProtocolVersion;
                var maxPlayers = MultiplayerManager.instance.maxPlayerCount;
                var options = new CreateLobbyOptions()
                {
                    IsPrivate = isPrivate,
                    Data = new Dictionary<string, DataObject>
                    {
                        {
                            KeyRelayJoinCode,
                            new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode)
                        },
                        {
                            KeyProtocolVersion,
                            new DataObject(DataObject.VisibilityOptions.Public, protocolVersion.ToString())
                        },
                        {
                            KeyHostBuildVersion,
                            new DataObject(DataObject.VisibilityOptions.Public, Application.version)
                        }
                    }
                };
                OnCreatingLobby.Invoke(new CreatingLobbyArgs
                    { Manager = this, Status = CreatingLobbyStatus.LobbyCreation });
                _currentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
                NetworkManager.Singleton.GetComponent<UnityTransport>()
                    .SetRelayServerData(allocation.ToRelayServerData("wss"));
                _lastHeartbeat = DateTime.Now;
                OnCreatingLobby.Invoke(new CreatingLobbyArgs
                    { Manager = this, Status = CreatingLobbyStatus.StartHost });
                MultiplayerManager.instance.StartHost();
                return _currentLobby;
            }
            catch (LobbyServiceException e)
            {
                Debug.LogException(e);
                OnCreateLobbyFailed.Invoke(new LobbyExceptionArgs
                {
                    Manager = this,
                    Exception = e,
                    Message = "Fail to create lobby"
                });
            }

            return null;
        }


        public async UniTask<Lobby> UpdateData(Lobby lobby, Dictionary<string, DataObject> newData)
        {
            try
            {
                return await LobbyService.Instance.UpdateLobbyAsync(lobby.Id,
                    new UpdateLobbyOptions { Data = newData });
            }
            catch (LobbyServiceException e)
            {
                Debug.LogException(e);
                return null;
            }
        }

        private bool DataKeyExists(Lobby lobby, string dataKey)
            => lobby.Data != null && lobby.Data.ContainsKey(dataKey);

        private string GetHostProtocolVersion(Lobby lobby)
        {
            return !DataKeyExists(lobby, KeyProtocolVersion)
                   || !LobbyExists(lobby)
                ? "0"
                : lobby.Data[KeyProtocolVersion].Value;
        }

        private bool IsNetworkCompatible(Lobby lobby)
            => LobbyExists(lobby)
               && GetHostProtocolVersion(lobby) == NetworkManager.Singleton.NetworkConfig.ProtocolVersion.ToString();

        public async UniTask<JoinAllocation> JoinLobby(Lobby lobby)
        {
            try
            {
                _currentLobby = lobby;
                if (!IsNetworkCompatible(_currentLobby))
                {
                    OnRefusedToJoinLobby.Invoke(new LobbyExceptionArgs
                    {
                        Manager = this,
                        Message = "Incompatible network version"
                    });
                    await LeaveLobby();
                    return null;
                }

                var relayJoinCode = _currentLobby.Data[KeyRelayJoinCode].Value;
                OnJoiningLobby.Invoke(new JoiningLobbyArgs
                    { Manager = this, Status = JoiningLobbyStatus.JoinAllocation });
                var joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);
                NetworkManager.Singleton.GetComponent<UnityTransport>()
                    .SetRelayServerData(joinAllocation.ToRelayServerData("wss"));
                OnJoiningLobby.Invoke(new JoiningLobbyArgs
                    { Manager = this, Status = JoiningLobbyStatus.StartClient });
                MultiplayerManager.instance.StartClient();
                return joinAllocation;
            }
            catch (LobbyServiceException e)
            {
                Debug.LogException(e);
                OnJoinLobbyByIdFailed.Invoke(new LobbyExceptionArgs
                {
                    Manager = this,
                    Exception = e
                });
            }

            return null;
        }

        private async UniTask LeaveLobby()
        {
            if (!LobbyExists(_currentLobby)) return;
            await LobbyService.Instance.RemovePlayerAsync(_currentLobby.Id,
                AuthenticationService.Instance.PlayerId);
            _currentLobby = null;
        }
    }
}