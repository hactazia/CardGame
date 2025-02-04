using System;
using UnityEngine;

namespace CardGameVR.Cards
{
    public class CardVisualManager : MonoBehaviour
    {
        public static CardVisualManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }
    }
}