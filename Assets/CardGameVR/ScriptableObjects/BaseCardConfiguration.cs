using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace CardGameVR.ScriptableObjects
{
    [CreateAssetMenu(fileName = "BaseCardConfiguration", menuName = "CardGameVR/Base Card Configuration"), Serializable]
    public class BaseCardConfiguration : ScriptableObject
    {
        public float drawChances = 1f;
        public uint maxPresence = 4;
    }
}