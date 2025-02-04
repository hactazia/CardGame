using Unity.Netcode;
using UnityEngine.Events;

namespace CardGameVR.Multiplayer
{
    public class PlayerDataChangedEvent : UnityEvent<PlayerDataChangedArgs>
    {
    }

    public class PlayerDataChangedArgs
    {
        public MultiplayerManager Manager;
        public NetworkListEvent<PlayerData> ChangeEvent;
    }
}