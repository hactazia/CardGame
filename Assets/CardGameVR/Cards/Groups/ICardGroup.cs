using System.Collections.Generic;
using System.Linq;
using CardGameVR.Cards.Slots;

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
    }
}