using System;
using UnityEngine;

namespace CardGameVR.ScriptableObjects
{
    [CreateAssetMenu(fileName = "BaseCardConfiguration", menuName = "CardGameVR/Configuration/Jumper"), Serializable]
    public class JumperConfiguration : BaseCardConfiguration
    {
        public float[] passiveEffect = { 0, 0.01f, -0.01f };
    }
}