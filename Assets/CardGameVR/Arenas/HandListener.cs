/*using System.Linq;
using CardGameVR.Cards;
using CardGameVR.Cards.Groups;
using CardGameVR.Cards.Slots;
using UnityEngine;

namespace CardGameVR.Arenas
{
    public class HandListener : MonoBehaviour
    {
        public ArenaPlacement ArenaPlacement;
        public BoardListener BoardListener;

        public CardSlot SelectedCard;

        public void Initialize(ArenaPlacement arenaPlacement)
        {
            ArenaPlacement.handCardGroup.OnSlotAdded.AddListener(OnSlotUpdate);
        }

        public void Dispose()
        {
            if (!ArenaPlacement) return;
            ArenaPlacement.handCardGroup.OnSlotAdded.RemoveListener(OnSlotUpdate);
            foreach (var slot in ArenaPlacement.handCardGroup.GetSlots())
                slot.onSelect.RemoveListener(OnCardSelect);
        }

        public void OnSlotUpdates()
        {
            foreach (var slot in ArenaPlacement.handCardGroup.GetSlots())
                OnSlotUpdate(slot);
        }

        public void OnSlotUpdate(CardSlot slot)
        {
            slot.interactable = ArenaPlacement.IsYourTurn;
            if (slot.interactable)
                slot.onSelect.AddListener(OnCardSelect);
            else slot.onSelect.RemoveListener(OnCardSelect);
        }

        private void OnCardSelect(CardSlot slot, bool isSelected)
        {
            if (!ArenaPlacement.IsYourTurn) return;

            if (isSelected && slot != SelectedCard)
            {
                Select(slot);
                BoardListener.Deselect();
                return;
            }

            if (!isSelected && slot == SelectedCard)
                Deselect();
        }

        public void Select(CardSlot slot)
        {
            foreach (var s in ArenaPlacement.handCardGroup
                         .GetSlots()
                         .Where(s => s.isSelected && s != slot))
                s.OnDeselect();

            Debug.Log($"Selected card: {slot.Card}");
            SelectedCard = slot;
        }

        public void Deselect()
        {
            if (!SelectedCard) return;
            Debug.Log($"Deselected card: {SelectedCard}");
            SelectedCard = null;
        }
    }
}*/