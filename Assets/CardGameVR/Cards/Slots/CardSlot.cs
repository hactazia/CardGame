using System;
using CardGameVR.Cards.Groups;
using CardGameVR.Cards.Types;
using CardGameVR.Cards.Visual;
using CardGameVR.Interactions;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace CardGameVR.Cards.Slots
{
    [RequireComponent(typeof(BoxCollider))]
    public class CardSlot : Interactable
    {
        public bool interactable = true;
        public ICard Card;

        public ICardGroup Group;

        public void Start() => OnRectTransformDimensionsChange();

        public override void OnHoverEnter()
        {
            if (!interactable) return;
            Card?.OnPointerEnter();
        }

        public override void OnHoverExit()
        {
            if (!interactable) return;
            Card?.OnPointerExit();
        }

        public override void OnSelect()
        {
            if (!interactable) return;
            Card?.OnPointerUp();
        }

        public override void OnDeselect()
        {
            if (!interactable) return;
            Card?.OnPointerDown();
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
            Card = c;
            if (c == null) return;
            c.GetTransform().SetParent(transform);
            c.GetTransform().localPosition = c.IsSelected
                ? c.GetSelectionOffset()
                : Vector3.zero;
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(CardSlot))]
    public class CardSlotEditor : EventInteractableEditor
    {
        public override void OnInspectorGUI()
        {
            var slot = (CardSlot)target;
            UnityEditor.EditorGUILayout.LabelField("Group", slot.Group == null ? "null" : slot.Group.ToString());
            UnityEditor.EditorGUILayout.LabelField("Card", slot.Card == null ? "null" : slot.Card.ToString());
        }
    }
#endif
}