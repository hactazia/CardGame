using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using CardGameVR.Cards;
using CardGameVR.Cards.Slots;
using CardGameVR.Cards.Visual;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace CardGameVR.Cards.Groups
{
    [RequireComponent(typeof(GridLayoutGroup))]
    public class GridCardGroup : MonoBehaviour, ICardGroup
    {
        public CardSlot cardSlotPrefab;

        [Header("Spawn Settings")] [SerializeField]
        public Vector2Int gridDimension = new(4, 4);

        public VisualCardHandler visualCardHandler;

        private bool _isCrossing = false;
        public CardSlot[] slots;

        private IEnumerator Frame()
        {
            yield return new WaitForSecondsRealtime(.1f);
            foreach (var s in slots)
                s.CardVisual?.UpdateIndex();
        }


        public virtual void Start()
        {
            OnRectTransformDimensionsChange();

            for (var i = 0; i < gridDimension.x * gridDimension.y; i++)
            {
                var s = Instantiate(cardSlotPrefab.gameObject, transform);
                s.name = $"[Slot] {i}";
            }

            slots = GetComponentsInChildren<CardSlot>().ToArray();
            foreach (var slot in slots)
                slot.Group = this;


            StartCoroutine(Frame());
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
            OnSlotRemoved.Invoke(slot);
            return true;
        }

        public void Clear()
        {
            foreach (var slot in slots.ToArray())
                Remove(slot);
        }

        public int SlotCount() => slots.Length;
        public int IndexOf(CardSlot slot) => Array.IndexOf(slots, slot);
        public CardSlot GetSlot(int index) => slots[index];

        public bool Has(CardSlot slot) => Array.IndexOf(slots, slot) != -1;

        public CardSlot[] GetSlots()
        {
            var list = new List<CardSlot>();
            for (var i = 0; i < SlotCount(); i++)
                list.Add(GetSlot(i));
            return list.ToArray();
        }

        public ICard[] GetCards()
            => (from slot in GetSlots() where slot.Card != null select slot.Card)
                .ToArray();

        public UnityEvent<CardSlot> OnSlotAdded { get; } = new();
        public UnityEvent<CardSlot> OnSlotRemoved { get; } = new();
        public UnityEvent<ICard, CardSlot> OnCardAdded { get; } = new();
        public UnityEvent<ICard, CardSlot> OnCardRemoved { get; } = new();
        public UnityEvent<CardSlot, bool> OnSelect { get; } = new();
        public UnityEvent<CardSlot, bool> OnHover { get; } = new();
    }
}