using CardGameVR.Cards;
using Unity.Netcode;
using UnityEngine;

namespace CardGameVR.Board
{
    public interface ISlot
    {
        public ICard GetCard();
        public void SetCard(ICard card);
        public bool HasCard();
        public void ClearCard();

        public bool TryGetCard(out ICard c)
        {
            c = GetCard();
            return HasCard();
        }
        
        public Transform GetTransform();
    }
}