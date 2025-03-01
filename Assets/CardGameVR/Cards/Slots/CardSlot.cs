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
    public class CardSlot : EventInteractable
    {
        public ICard Card;

        public ICardGroup Group;

        public void Start()
        {
            OnRectTransformDimensionsChange();

            onSelectEvent.AddListener(() => AddCard().Forget());
        }

        private async UniTask AddCard()
        {
            if (Card != null) return;
            Group.Set(this, await TestCard.Create());
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
            Debug.Log($"Set card {c} to slot {this}");
            Debug.Log($"a {c.GetTransform()}");
            Debug.Log($"b {transform}");
            c.GetTransform().SetParent(transform);
            c.GetTransform().localPosition = c.IsSelected()
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