using UnityEngine.Events;

namespace CardGameVR.Multiplayer
{
    public class TryToConnectEvent : UnityEvent<TryToConnectArgs>
    {
    }

    public class TryToConnectArgs
    {
        public MultiplayerManager Manager;
    }
}