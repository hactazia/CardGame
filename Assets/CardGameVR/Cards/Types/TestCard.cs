using System;
using CardGameVR.Cards.Slots;
using CardGameVR.Cards.Visual;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace CardGameVR.Cards.Types
{
    public class TestCard : MonoBehaviour, ICard
    {
        private CardSlot _slot;
        private int _id;

        public TestCard()
        {
            _id = GetInstanceID();
        }

        public string GetCardType() => "test";

        public int GetId() => _id;
        public void SetId(int id) => _id = id;
        public int[] CanMoveTo() => Array.Empty<int>();

        [Header("Visuals")] [SerializeField] public VisualCard visualCardPrefab;
        public VisualCard visualCard;

        public float selectionOffset = 10f;

        [Header("States")] public bool isSelected;
        public bool isDragging;
        public bool wasDragged;
        public bool isHovering;

        public UnityEvent<ICard> PointerEnterEvent { get; } = new();
        public UnityEvent<ICard> PointerExitEvent { get; } = new();
        public UnityEvent<ICard> BeginDragEvent { get; } = new();
        public UnityEvent<ICard> EndDragEvent { get; } = new();
        public UnityEvent<ICard, bool> SelectEvent { get; } = new();
        public UnityEvent<ICard, bool> PointerUpEvent { get; } = new();
        public UnityEvent<ICard> PointerDownEvent { get; } = new();

        public void OnPointerEnter()
        {
            PointerEnterEvent.Invoke(this);
            isHovering = true;
        }

        public void OnPointerExit()
        {
            PointerExitEvent.Invoke(this);
            isHovering = false;
        }

        public void OnPointerDown()
        {
            PointerDownEvent.Invoke(this);
        }

        public void OnPointerUp()
        {
            if (wasDragged)
                return;

            isSelected = !isSelected;
            SelectEvent.Invoke(this, isSelected);

            if (isSelected)
                transform.localPosition += transform.up * selectionOffset;
            else
                transform.localPosition = Vector3.zero;
        }

        public Transform GetTransform() => transform;

        public void SetSlot(CardSlot slot) => _slot = slot;
        public CardSlot GetSlot() => _slot;

        public VisualCard SpawnVisualCard(VisualCardHandler handler)
        {
            if (!handler)
                throw new System.Exception("VisualCardHandler is null");

            Debug.Log($"Spawning Visual Card: {visualCardPrefab}");

            var go = Instantiate(visualCardPrefab.gameObject, handler.transform);
            visualCard = go.GetComponent<VisualCard>();
            visualCard.Initialize(this);
            return visualCard;
        }

        public bool TryGetVisualCard(out VisualCard visual)
        {
            visual = visualCard;
            return visual;
        }


        public bool IsSelected
        {
            get => isSelected;
            set => isSelected = value;
        }

        public bool IsDragging
        {
            get => isDragging;
            set => isDragging = value;
        }

        public bool WasDragged
        {
            get => wasDragged;
            set => wasDragged = value;
        }

        public bool IsHovering
        {
            get => isHovering;
            set => isHovering = value;
        }

        public Vector3 GetSelectionOffset() => Vector3.zero;

        public void OnDestroy()
        {
            if (TryGetVisualCard(out var cardVisual))
                Destroy(cardVisual.gameObject);
        }

        public static async UniTask<TestCard> Create()
        {
            var go = await Addressables.LoadAssetAsync<GameObject>("TestCard");
            var instance = Instantiate(go);
            return instance.GetComponent<TestCard>();
        }
    }
}