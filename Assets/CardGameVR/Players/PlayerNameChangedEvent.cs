using UnityEngine.Events;

namespace CardGameVR.Players
{
    public class PlayerNameChangedEvent : UnityEvent<PlayerNameChangedArgs>
    {
    }
    
    public class PlayerNameChangedArgs
    {
        public PlayerController Player;
        public string OldName;
        public string NewName;
    }
}