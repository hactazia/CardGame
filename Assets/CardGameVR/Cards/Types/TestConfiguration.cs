using System;
using UnityEngine;

namespace CardGameVR.ScriptableObjects
{
    [CreateAssetMenu(fileName = "BaseCardConfiguration", menuName = "CardGameVR/Configuration/Test"), Serializable]
    public class TestConfiguration : BaseCardConfiguration
    {
        public float removePercentage = 0.01f;
    }
}