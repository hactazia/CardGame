using UnityEngine.Events;

namespace CardGameVR.Lobbies
{
    public class CreatingLobbyEvent : UnityEvent<CreatingLobbyArgs>
    {
    }

    public class CreatingLobbyArgs
    {
        public LobbyManager Manager;
        public CreatingLobbyStatus Status;
    }

    public enum CreatingLobbyStatus
    {
        Preparing,
        RelayAllocation,
        RelayJoinCode,
        LobbyCreation,
        StartHost
    }
}