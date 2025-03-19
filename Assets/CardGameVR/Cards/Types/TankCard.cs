using System;
using System.Collections.Generic;
using System.Linq;
using CardGameVR.Arenas;
using CardGameVR.Cards.Slots;
using CardGameVR.Cards.Visual;
using CardGameVR.Players;
using CardGameVR.ScriptableObjects;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

namespace CardGameVR.Cards.Types
{
    public class TankCard : MonoBehaviour, ICard
    {
        public static string GetTypeName() => "tank";

        public static TankConfiguration GetGlobalConfiguration()
            => Resources.Load<TankConfiguration>(GetTypeName()+"_configuration");
        public BaseCardConfiguration GetConfiguration()
            => GetGlobalConfiguration();
        public float[] GetPassiveEffect(NetworkPlayer player, bool recursive = true)
        {
            List<float> effects = new();
            for (var i = 0; i < player.Lives.Length; i++)
                effects.Add(0);
            return effects.ToArray();
        }
        public float[] GetActiveEffect(bool recursive = true)
        {
            var player = this.GetOwner();
            if (!this.TryBoard(out _) || !recursive)
                return GetPassiveEffect(player);
            var effects = GetPassiveEffect(player).ToList();
            foreach(var move in CanMoveTo())
                if (BoardGroup.TryGetCard(move, out var card))
                {
                    var cardEffects = card.GetPassiveEffect(player, false);
                    for(var i = 0; i < cardEffects.Length; i++)
                        effects[i] -= cardEffects[i] * GetGlobalConfiguration().takeDamagePercentage;
                }
            return effects.ToArray();
        }
        
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

        [Header("States")] public bool isDragging;
        public bool wasDragged;

        [SerializeField] public VisualCard visualCardPrefab;

        public UnityEvent<ICard> PointerEnterEvent { get; } = new();
        public UnityEvent<ICard> PointerExitEvent { get; } = new();
        public UnityEvent<ICard> BeginDragEvent { get; } = new();
        public UnityEvent<ICard> EndDragEvent { get; } = new();
        public UnityEvent<ICard, bool> SelectEvent { get; } = new();
        public UnityEvent<ICard, bool> PointerUpEvent { get; } = new();
        public UnityEvent<ICard> PointerDownEvent { get; } = new();

        public void OnPointerEnter() => PointerEnterEvent.Invoke(this);
        public void OnPointerExit() => PointerExitEvent.Invoke(this);
        public void OnPointerDown() => PointerDownEvent.Invoke(this);

        public void OnPointerUp()
        {
            if (!_slot) return;
            SelectEvent.Invoke(this, _slot.isSelected);
        }

        private void Update()
        {
            if (!_slot) return;
            transform.localPosition = _slot.isSelected
                ? transform.up * selectionOffset
                : Vector3.zero;
        }


        public int GetId() => _id;

        public void SetId(int id) => _id = id;
        public bool IsBoosted() => this.TryBoard(out var board) && board.IsBoosted;

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

        public int[] CanMoveTo()
        {
            if (!this.TryBoard(out _))
                return Array.Empty<int>();

            //  X
            // XCX
            //  X

            Vector2Int[] list =
            {
                new(0, 1),
                new(1, 0),
                new(0, -1),
                new(-1, 0),
                new(0, 0)
            };
            var pos = this.GetCell();
            var moves = (from move in list
                    select pos + move
                    into target
                    where BoardGroup.IsInBounds(target)
                    select BoardGroup.GetIndex(target))
                .ToList();

            return moves.ToArray();
        }

        public Vector3 GetSelectionOffset() => Vector3.zero;

        public void OnDestroy()
        {
            Debug.Log($"Destroying {this}");
            if (TryGetVisualCard(out var cardVisual))
                Destroy(cardVisual.gameObject);
        }
    }
}