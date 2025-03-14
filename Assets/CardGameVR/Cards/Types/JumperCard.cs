using CardGameVR.Cards.Slots;
using CardGameVR.Cards.Visual;
using UnityEngine;
using UnityEngine.Events;

namespace CardGameVR.Cards.Types
{
    public class TankCard : MonoBehaviour, ICard
    {
        public static string GetTypeName() => "tank";
        
        private uint _id;
        private CardSlot _slot;
        private VisualCard _visualCard;

        public TankCard()
        {
            _id = (uint)GetInstanceID();
        }
        
        [SerializeField] public VisualCard visualCardPrefab;

        public UnityEvent<ICard> PointerEnterEvent { get; } = new();
        public UnityEvent<ICard> PointerExitEvent { get; } = new();
        public UnityEvent<ICard> BeginDragEvent { get; } = new();
        public UnityEvent<ICard> EndDragEvent { get; } = new();
        public UnityEvent<ICard, bool> SelectEvent { get; } = new();
        public UnityEvent<ICard, bool> PointerUpEvent { get; } = new();
        public UnityEvent<ICard> PointerDownEvent { get; } = new();

        public uint GetId() => _id;

        public void SetId(uint id) => _id = id;

        public Transform GetTransform() => transform;


        public void SetSlot(CardSlot slot) => _slot = slot;
        public CardSlot GetSlot() => _slot;

        public VisualCard SpawnVisualCard(VisualCardHandler handler)
        {
            if (!handler)
                throw new System.Exception("VisualCardHandler is null");

            Debug.Log($"Spawning Visual Card: {visualCardPrefab}");

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

        public bool IsSelected()
        {
            throw new System.NotImplementedException();
        }

        public bool IsDragging()
        {
            throw new System.NotImplementedException();
        }

        public bool WasDragged()
        {
            throw new System.NotImplementedException();
        }

        public bool IsHovering()
        {
            throw new System.NotImplementedException();
        }

        public Vector3 GetSelectionOffset()
        {
            throw new System.NotImplementedException();
        }

    }
}