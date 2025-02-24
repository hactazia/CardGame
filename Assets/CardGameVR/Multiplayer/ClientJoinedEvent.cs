using UnityEngine.Events;

namespace CardGameVR.Multiplayer
{
    public class ClientJoinedEvent : UnityEvent<ClientJoinedArgs>
    {
    }
    
    public class ClientJoinedArgs
    {
        public ulong ClientId;
    }
}