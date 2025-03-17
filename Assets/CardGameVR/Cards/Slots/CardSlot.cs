using System;
using CardGameVR.Cards.Groups;
using CardGameVR.Cards.Types;
using CardGameVR.Cards.Visual;
using CardGameVR.Interactions;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace CardGameVR.Cards.Slots
{
    [RequireComponent(typeof(BoxCollider))]
    public class CardSlot : Interactable
    {
        public bool interactable = true;
        public ICard Card;
        private ICardGroup _group;
        public Animator animator;

        public ICardGroup Group
        {
            get => _group;
            set
            {
                if (_group == value) return;
                _group?.OnSlotRemoved.Invoke(this);
                _group = value;
                value?.OnSlotAdded.Invoke(this);
            }
        }

        public int Index => Group?.IndexOf(this) ?? -1;

        public bool isSelected = false;
        public bool isHovering = false;

        public UnityEvent<CardSlot, bool> onSelect = new();
        public UnityEvent<CardSlot, bool> onHover = new();

        public void Start() => OnRectTransformDimensionsChange();

        public override void OnHoverEnter()
        {
            if (!interactable) return;
            isHovering = true;
            var old = isHovering;
            Card?.OnPointerEnter();
            if (old != isHovering)
            {
                onHover.Invoke(this, isHovering);
                Group?.OnHover.Invoke(this, isHovering);
            }
        }

        public override void OnHoverExit()
        {
            if (!interactable) return;
            var old = isHovering;
            isHovering = false;
            Card?.OnPointerExit();
            if (old != isHovering)
            {
                onHover.Invoke(this, isHovering);
                Group?.OnHover.Invoke(this, isHovering);
            }
        }

        public override void OnSelect()
        {
            if (!interactable) return;

            Group?.OnSelect.Invoke(this, true);
            onSelect.Invoke(this, true);
            Card?.OnPointerDown();
        }

        public override void OnDeselect()
        {
            if (!interactable) return;
            Group?.OnSelect.Invoke(this, false);
            onSelect.Invoke(this, false);
            Card?.OnPointerUp();
        }

        private void OnRectTransformDimensionsChange()
        {
            var rect = GetComponent<RectTransform>();
            var col = GetComponent<BoxCollider>();
            col.size = new Vector3(
                rect.rect.width,
                rect.rect.height,
                col.size.z
            );
        }

        public VisualCard CardVisual
            => Card != null && Card.TryGetVisualCard(out var visualCard)
                ? visualCard
                : null;

        public void SetCard(ICard c)
        {
            if (Card != null)
            {
                Card.SetSlot(null);
                Group?.OnCardRemoved.Invoke(Card, this);
            }

            Card = c;
            if (c == null) return;
            Card.SetSlot(this);
            Group?.OnCardAdded.Invoke(Card, this);
            c.GetTransform().SetParent(transform);
            c.GetTransform().localPosition = c.IsSelected
                ? c.GetSelectionOffset()
                : Vector3.zero;
        }

        private void OnDestroy()
        {
            SetCard(null);
            Group?.OnSlotRemoved.Invoke(this);
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(CardSlot))]
    public class CardSlotEditor : EventInteractableEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var slot = (CardSlot)target;
            UnityEditor.EditorGUILayout.LabelField("Group", slot.Group == null ? "null" : slot.Group.ToString());
            UnityEditor.EditorGUILayout.LabelField("Card", slot.Card == null ? "null" : slot.Card.ToString());
            UnityEditor.EditorGUILayout.Toggle("Is Selected", slot.isSelected);
            UnityEditor.EditorGUILayout.Toggle("Is Hovering", slot.isHovering);
            UnityEditor.EditorGUILayout.Toggle("Interactable", slot.interactable);
        }
    }
#endif
}