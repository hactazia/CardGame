using Unity.Services.Lobbies;
using UnityEngine.Events;

namespace CardGameVR.Lobbies
{
    public class LobbyExceptionEvent : UnityEvent<LobbyExceptionArgs>
    {
    }

    public class LobbyExceptionArgs
    {
        public LobbyManager Manager;
        public LobbyServiceException Exception;
        public string Message;
    }
}