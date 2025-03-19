using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace CardGameVR.ScriptableObjects
{
    [CreateAssetMenu(fileName = "PartyConfiguration", menuName = "CardGameVR/Party Configuration"), Serializable]
    public class PartyConfiguration : ScriptableObject
    {
        public int minPlayers = 2;
        public byte numberOfLives = 4;
        public int maxCardInHand = 8;
        public int initialNumberInHand = 4;
        public float effectMultiplier = 1f;
        public float initialHealth = 0f;
        public Vector2 healthRange = new(-1f, 1f);
        public float boostRarity = 0.05f;
        public int maxBoosts = 3;
    }
}