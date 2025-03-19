using System;
using UnityEngine;

namespace CardGameVR.ScriptableObjects
{
    [CreateAssetMenu(fileName = "PartyConfiguration", menuName = "CardGameVR/Party Configuration"), Serializable]
    public class PartyConfiguration : ScriptableObject
    {
        public int minPlayers = 2;
        public byte numberOfLives = 4;
        public int maxCardInHand = 8;
        public int initialNumberInHand = 4;
    }
}