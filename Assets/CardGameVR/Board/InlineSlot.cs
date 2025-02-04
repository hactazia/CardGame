using CardGameVR.Cards;
using UnityEngine;

namespace CardGameVR.Board
{
    public class InlineSlot : MonoBehaviour, ISlot
    {
        private ICard _card;

        public ICard GetCard() => _card;

        public void SetCard(ICard card)
        {
            _card = card;
            var cardTransform = card.GetTransform();
            cardTransform.SetParent(transform);
            cardTransform.localPosition = Vector3.zero;
        }
        public bool HasCard() => _card != null;
        public void ClearCard() => _card = null;
        public Transform GetTransform() => transform;
    }
}