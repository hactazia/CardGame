using System.Collections.Generic;
using System.Linq;
using CardGameVR.Cards.Slots;
using UnityEngine.Events;

namespace CardGameVR.Cards.Groups
{
    public interface ICardGroup
    {
        public bool Add(ICard card);
        public bool Set(CardSlot slot, ICard card);
        public bool Remove(ICard card);
        public void Clear();

        public int SlotCount();
        public int IndexOf(CardSlot slot);
        public CardSlot GetSlot(int index);
        public bool Has(CardSlot slot);

        public CardSlot[] GetSlots();
        public ICard[] GetCards();

        public UnityEvent<CardSlot> OnSlotAdded { get; }
        public UnityEvent<CardSlot> OnSlotRemoved { get; }
        public UnityEvent<ICard, CardSlot> OnCardAdded { get; }
        public UnityEvent<ICard, CardSlot> OnCardRemoved { get; }
        public UnityEvent<CardSlot, bool> OnSelect { get; }
        public UnityEvent<CardSlot, bool> OnHover { get; }

        static void Swap(CardSlot slot1, CardSlot slot2)
        {
            var card1 = slot1.Card;
            var card2 = slot2.Card;
            slot1.SetCard(card2);
            slot2.SetCard(card1);
        }
    }
}