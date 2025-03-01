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
        
        [Header("Visuals")]
        [SerializeField] public VisualCard visualCardPrefab;
        public VisualCard visualCard;
        
        [Header("Settings")]
        public bool reCenterOnDragged = true;
        
        [Header("States")]
        public bool isSelected;
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
        
        
        public bool IsSelected() => isSelected;
        public bool IsDragging() => isDragging;
        public bool WasDragged() => wasDragged;
        public bool IsHovering() => isHovering;

        public Vector3 GetSelectionOffset() => Vector3.zero;
        
        public static async UniTask<TestCard> Create()
        {
            var go = await Addressables.LoadAssetAsync<GameObject>("TestCard");
            var instance = Instantiate(go);
            return instance.GetComponent<TestCard>();
        }
    }
}