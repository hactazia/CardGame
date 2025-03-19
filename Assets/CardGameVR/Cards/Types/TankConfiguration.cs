using System;
using UnityEngine;

namespace CardGameVR.ScriptableObjects
{
    [CreateAssetMenu(fileName = "BaseCardConfiguration", menuName = "CardGameVR/Configuration/Tank"), Serializable]
    public class TankConfiguration : BaseCardConfiguration
    {
        public float takeDamagePercentage = 0.1f;
    }
}