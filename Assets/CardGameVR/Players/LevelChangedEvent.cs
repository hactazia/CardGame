using UnityEngine;
using UnityEngine.Events;

namespace CardGameVR.Players
{
    public class LevelChangedEvent : UnityEvent<LevelChangedArgs>
    {
    }

    public class LevelChangedArgs
    {
        public PlayerController Player;
        public Vector4 OldLevels;
        public Vector4 NewLevels;
    }
}