using System;
using CardGameVR.Cards.Slots;
using CardGameVR.Cards.Visual;
using CardGameVR.Parties;
using CardGameVR.Players;
using CardGameVR.ScriptableObjects;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace CardGameVR.Cards
{
    public interface ICard
    {
        string GetCardType();
        public int GetId();
        public void SetId(int id);
        
        public int[] CanMoveTo();
        public async UniTask<BaseCardConfiguration> GetConfiguration();

        public Transform GetTransform();
        public void SetSlot(CardSlot slot);
        public CardSlot GetSlot();
        public VisualCard SpawnVisualCard(VisualCardHandler handler);
        public bool TryGetVisualCard(out VisualCard visual);
        public bool IsSelected => GetSlot()?.isSelected ?? false;
        public bool IsHovering => GetSlot()?.isHovering ?? false;
        public Vector3 GetSelectionOffset();

        public UnityEvent<ICard> PointerEnterEvent { get; }
        public UnityEvent<ICard> PointerExitEvent { get; }
        public UnityEvent<ICard> BeginDragEvent { get; }
        public UnityEvent<ICard> EndDragEvent { get; }
        public UnityEvent<ICard, bool> SelectEvent { get; }
        public UnityEvent<ICard, bool> PointerUpEvent { get; }
        public UnityEvent<ICard> PointerDownEvent { get; }

        public void OnPointerEnter();
        public void OnPointerExit();
        public void OnPointerDown();
        public void OnPointerUp();

    }
}