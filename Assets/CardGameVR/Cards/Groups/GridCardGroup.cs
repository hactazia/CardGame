using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using CardGameVR.Cards;
using CardGameVR.Cards.Slots;
using CardGameVR.Cards.Visual;

namespace CardGameVR.Cards.Groups
{
    [RequireComponent(typeof(GridLayoutGroup))]
    public class GridCardGroup : MonoBehaviour, ICardGroup
    {
        [SerializeField] private ICard _selectedCard;
        [SerializeField] private ICard _hoveredCard;

        public CardSlot cardSlotPrefab;

        [Header("Spawn Settings")] [SerializeField]
        private Vector2Int gridDimension = new(4, 4);

        public VisualCardHandler visualCardHandler;

        private bool _isCrossing = false;
        public CardSlot[] slots;

        private IEnumerator Frame()
        {
            yield return new WaitForSecondsRealtime(.1f);
            foreach (var s in slots)
                s.CardVisual?.UpdateIndex();
        }

        void CardPointerEnter(ICard card)
            => _hoveredCard = card;

        void CardPointerExit(ICard card)
            => _hoveredCard = null;

        private void BeginDrag(ICard card)
            => _selectedCard = card;

        void Start()
        {
            OnRectTransformDimensionsChange();

            for (var i = 0; i < gridDimension.x * gridDimension.y; i++)
            {
                var s = Instantiate(cardSlotPrefab.gameObject, transform);
                s.name = $"[Slot] {i}";
            }

            slots = GetComponentsInChildren<CardSlot>().ToArray();
            
            foreach (var slot in slots)
            {
                slot.Group = this;
                if (slot.Card == null) continue;
                slot.Card.PointerEnterEvent.AddListener(CardPointerEnter);
                slot.Card.PointerExitEvent.AddListener(CardPointerExit);
                slot.Card.BeginDragEvent.AddListener(BeginDrag);
                slot.Card.EndDragEvent.AddListener(EndDrag);
            }

            StartCoroutine(Frame());
        }

        private void EndDrag(ICard card)
        {
            // ..
        }

        private void OnRectTransformDimensionsChange()
        {
            var grid = GetComponent<GridLayoutGroup>();
            var rect = GetComponent<RectTransform>();

            grid.cellSize = new Vector2(
                rect.rect.width / gridDimension.x,
                rect.rect.height / gridDimension.y
            );
        }

        private CardSlot GetSlotFromCard(ICard card)
            => slots.FirstOrDefault(slot => slot.Card == card);

        void Swap(ICard focused, ICard crossed)
        {
            _isCrossing = true;

            var focusedParent = GetSlotFromCard(focused);
            var crossedParent = GetSlotFromCard(crossed);

            focusedParent.SetCard(crossed);
            crossedParent.SetCard(focused);

            _isCrossing = false;

            if (!crossed.TryGetVisualCard(out var crossedVisual))
                return;

            crossedVisual.Swap(focused.GetTransform().position.x > crossed.GetTransform().position.x ? 1 : -1);

            foreach (var s in slots)
                s.CardVisual?.UpdateIndex();
        }

        public bool Set(CardSlot slot, ICard card)
        {
            if (!Has(slot) || slot.Card != null) return false;
            slot.SetCard(card);
            Debug.Log($"Set {card} to {slot} (with {visualCardHandler ?? VisualCardHandler.Instance})");
            if (!card.TryGetVisualCard(out _))
                card.SpawnVisualCard(visualCardHandler ?? VisualCardHandler.Instance);

            foreach (var s in slots)
                s.CardVisual?.UpdateIndex();

            return true;
        }

        public bool Add(ICard c) => false;

        public bool Remove(ICard card) => Remove(GetSlotFromCard(card));

        private bool Remove(CardSlot slot)
        {
            if (!Has(slot) || slot.Card == null) 
                return false;
            slot.SetCard(null);
            return true;
        }

        public void Clear()
        {
            foreach (var slot in slots.ToArray())
                Remove(slot);
        }

        public int SlotCount() => slots.Length;
        public int IndexOf(CardSlot slot) => Array.IndexOf(slots, slot);
        public bool Has(CardSlot slot) => Array.IndexOf(slots, slot) != -1;
    }
}