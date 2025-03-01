using CardGameVR.Cards.Slots;
using CardGameVR.Cards.Visual;
using UnityEngine;
using UnityEngine.Events;

namespace CardGameVR.Cards
{
    public interface ICard
    {
        public Transform GetTransform();
        public void SetSlot(CardSlot slot);
        public CardSlot GetSlot();
        public VisualCard SpawnVisualCard(VisualCardHandler handler);
        public bool TryGetVisualCard(out VisualCard visual);
        public bool IsSelected();
        public bool IsDragging();
        public bool WasDragged();
        public bool IsHovering();
        public Vector3 GetSelectionOffset();

        public UnityEvent<ICard> PointerEnterEvent { get; }
        public UnityEvent<ICard> PointerExitEvent { get; }
        public UnityEvent<ICard> BeginDragEvent { get; }
        public UnityEvent<ICard> EndDragEvent { get; }
        public UnityEvent<ICard, bool> SelectEvent { get; }
        public UnityEvent<ICard, bool> PointerUpEvent { get; }
        public UnityEvent<ICard> PointerDownEvent { get; }
        
        public int GroupCount()
            => GetSlot() 
                ? GetSlot().Group.SlotCount() - 1 
                : 0;

        public int GroupIndex()
            => GetSlot() 
                ? GetSlot().Group.IndexOf(GetSlot()) 
                : 0;

        private void Destroy()
        {
            if (TryGetVisualCard(out var cardVisual))
                Object.Destroy(cardVisual.gameObject);
        }
    }
}