using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CardGameVR.Cards.Slots;
using CardGameVR.Cards.Visual;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CardGameVR.Cards.Groups
{
    [RequireComponent(typeof(HorizontalLayoutGroup))]
    public class HorizontalCardGroup : MonoBehaviour, ICardGroup
    {
        [SerializeField] private ICard selectedCard;
        [SerializeReference] private ICard hoveredCard;
        public VisualCardHandler visualCardHandler;

        [SerializeField] private CardSlot slotPrefab;

        [Header("Spawn Settings")] [SerializeField]
        private int cardsToSpawn = 7;

        public int maxCards = 15;

        public List<CardSlot> slots;

        bool _isCrossing = false;
        [SerializeField] private bool tweenCardReturn = true;

        public void Start()
        {
            StartCoroutine(Frame());
        }


        public void Clear()
        {
            foreach (var slot in slots.ToArray())
                Remove(slot);
        }

        public int CardCount() => slots.Count(slot => slot.Card != null);

        public int SlotCount() => slots.Count;
        public int IndexOf(CardSlot slot) => slots.IndexOf(slot);
        public CardSlot GetSlot(int index) => slots[index];

        public bool Has(CardSlot slot) => slots.Contains(slot);

        public bool Add(ICard card)
        {
            if (slots.Count >= maxCards)
                return false;
            var t = Instantiate(slotPrefab.gameObject, transform);
            t.name = $"[Slot] {slots.Count}";
            var slot = t.GetComponent<CardSlot>();
            slots.Add(slot);
            slot.Group = this;
            slot.SetCard(card);
            if (!card.TryGetVisualCard(out _))
                card.SpawnVisualCard(visualCardHandler ?? VisualCardHandler.Instance);
            foreach (var s in slots)
                s.CardVisual?.UpdateIndex();
            return true;
        }

        public bool Set(CardSlot slot, ICard card) => false;

        public bool Remove(ICard card)
        {
            var slot = GetSlotFromCard(card);
            return slot && Remove(slot);
        }

        public bool Remove(CardSlot slot)
        {
            slots.Remove(slot);
            Destroy(slot.gameObject);
            return true;
        }

        private IEnumerator Frame()
        {
            yield return new WaitForSecondsRealtime(.1f);
            foreach (var s in slots)
                s.CardVisual?.UpdateIndex();
        }

        private CardSlot GetSlotFromCard(ICard card)
            => slots.FirstOrDefault(slot => slot.Card == card);

        private void BeginDrag(ICard card)
            => selectedCard = card;

        void EndDrag(ICard card)
        {
            // ..
        }

        void CardPointerEnter(ICard card)
            => hoveredCard = card;

        void CardPointerExit(ICard card)
            => hoveredCard = null;

        void Swap(ICard focused, ICard crossed)
        {
            _isCrossing = true;

            var focusedParent = GetSlotFromCard(focused);
            var crossedParent = GetSlotFromCard(crossed);

            focusedParent.SetCard(crossed);
            crossedParent.SetCard(focused);

            _isCrossing = false;

            if (!crossed.TryGetVisualCard(out var crossedVisual)) return;

            crossedVisual.Swap(focused.GetTransform().position.x > crossed.GetTransform().position.x ? 1 : -1);

            foreach (var s in slots)
                s.CardVisual?.UpdateIndex();
        }

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