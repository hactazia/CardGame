using System.Collections.Generic;
using CardGameVR.Board;
using CardGameVR.Cards.Visual;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace CardGameVR.Cards.Groups
{
    [RequireComponent(typeof(HorizontalLayoutGroup))]
    public class HorizontalCardGroup : MonoBehaviour
    {
        public GameObject slotPrefab;
        public int maxCards = 5;
        public readonly List<ICard> Cards = new();
        public VisualCardHandler visualCardHandler;

        public void Clear()
        {
            foreach (Transform child in transform)
                Destroy(child.gameObject);
            Cards.Clear();
        }

        public void AddCard(ICard card)
        {
            if (Cards.Count >= maxCards) return;
            var slot = Instantiate(slotPrefab, transform).GetComponent<ISlot>();
            slot.SetCard(card);
            card.SpawnVisualCard(visualCardHandler ?? VisualCardHandler.Instance);
            Cards.Add(card);
        }

        public void RemoveCard(ICard card)
        {
            if (!Cards.Contains(card)) return;
            Cards.Remove(card);
            var slot = transform.GetComponentsInChildren<ISlot>();
            foreach (var s in slot)
                if (s.GetCard() == card)
                {
                    s.ClearCard();
                    Destroy(s.Destroy());
                    break;
                }
        }
    }
}