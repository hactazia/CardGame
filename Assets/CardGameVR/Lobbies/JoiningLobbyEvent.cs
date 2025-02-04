using UnityEngine.Events;

namespace CardGameVR.Lobbies
{
    public class JoiningLobbyEvent : UnityEvent<JoiningLobbyArgs>
    {
    }

    public class JoiningLobbyArgs
    {
        public LobbyManager Manager;
        public JoiningLobbyStatus Status;
    }

    public enum JoiningLobbyStatus
    {
        Preparing,
        JoinAllocation,
        StartClient
    }
}