using UnityEngine.Events;

namespace CardGameVR.Players
{
    public class PlayerStateChangedEvent : UnityEvent<PlayerStateChangedArgs>
    {
    }

    public class PlayerStateChangedArgs
    {
        public PlayerController Player;
        public PlayerState OldState;
        public PlayerState NewState;
    }
}