using UnityEngine.Events;

namespace CardGameVR.Multiplayer
{
    public class ClientLeftEvent : UnityEvent<ClientLeftArgs>
    {
    }
    
    public class ClientLeftArgs
    {
        public MultiplayerManager Manager;
        public ulong ClientId;
    }
}