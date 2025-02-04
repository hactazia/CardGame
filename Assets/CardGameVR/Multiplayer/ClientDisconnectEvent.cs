using UnityEngine.Events;

namespace CardGameVR.Multiplayer
{
    public class ClientDisconnectEvent : UnityEvent<ClientDisconnectArgs>
    {
    }
    
    public class ClientDisconnectArgs
    {
        public MultiplayerManager Manager;
    }
}