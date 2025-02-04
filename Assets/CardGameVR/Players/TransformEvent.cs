using UnityEngine;
using UnityEngine.Events;

namespace CardGameVR.Players
{
    public class TransformEvent : UnityEvent<TransformArgs>
    {
    }

    public class TransformArgs
    {
        public PlayerController Player;
        public Vector3 Position;
        public Quaternion Rotation;
    }
}