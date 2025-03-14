using System;
using UnityEngine;

namespace CardGameVR.ScriptableObjects
{
    [CreateAssetMenu(fileName = "PartyConfiguration", menuName = "CardGameVR/Party Configuration"), Serializable]
    public class PartyConfiguration : ScriptableObject
    {
        public int minPlayers = 2;
        public byte numberOfLives = 4;
        public uint maxCardInHand = 8;
        public uint initialNumberInHand = 4;
    }
}