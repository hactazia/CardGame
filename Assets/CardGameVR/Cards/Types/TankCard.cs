using CardGameVR.Cards.Slots;
using CardGameVR.Cards.Visual;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

namespace CardGameVR.Cards.Types
{
    public class TankCard : MonoBehaviour, ICard
    {
        public static string GetTypeName() => "tank";
        public const uint MaxPresence = uint.MaxValue;
        public const float DrawChances = 1f;

        public static async UniTask<TankCard> SpawnType()
        {
            var go = await Addressables.LoadAssetAsync<GameObject>("cards/" + GetTypeName());
            var instance = Instantiate(go);
            return instance.GetComponent<TankCard>();
        }

        public string GetCardType()
            => GetTypeName();

        private int _id;
        private CardSlot _slot;
        private VisualCard _visualCard;


        public float selectionOffset = 10f;

        [Header("States")] public bool isSelected;
        public bool isDragging;
        public bool wasDragged;
        public bool isHovering;

        [SerializeField] public VisualCard visualCardPrefab;

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


        public int GetId() => _id;

        public void SetId(int id) => _id = id;

        public Transform GetTransform() => transform;


        public void SetSlot(CardSlot slot) => _slot = slot;
        public CardSlot GetSlot() => _slot;

        public VisualCard SpawnVisualCard(VisualCardHandler handler)
        {
            if (!handler)
                throw new System.Exception("VisualCardHandler is null");

            var go = Instantiate(visualCardPrefab.gameObject, handler.transform);
            _visualCard = go.GetComponent<VisualCard>();
            _visualCard.Initialize(this);
            return _visualCard;
        }

        public bool TryGetVisualCard(out VisualCard visual)
        {
            if (!_visualCard)
            {
                visual = null;
                return false;
            }

            visual = _visualCard;
            return true;
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
    }
}