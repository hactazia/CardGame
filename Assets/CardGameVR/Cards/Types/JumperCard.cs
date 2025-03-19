using System;
using System.Collections.Generic;
using System.Linq;
using CardGameVR.Arenas;
using CardGameVR.Cards.Slots;
using CardGameVR.Cards.Visual;
using CardGameVR.Parties;
using CardGameVR.Players;
using CardGameVR.ScriptableObjects;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

namespace CardGameVR.Cards.Types
{
    public class JumperCard : MonoBehaviour, ICard
    {
        public static string GetTypeName() => "jumper";

        public static JumperConfiguration GetGlobalConfiguration()
            => Resources.Load<JumperConfiguration>(GetTypeName() + "_configuration");

        public BaseCardConfiguration GetConfiguration()
            => GetGlobalConfiguration();


        public int[] CanMoveTo()
        {
            if (!this.TryBoard(out _))
                return Array.Empty<int>();

            // XXXXX
            // X   X
            // X C X
            // X   X
            // XXXXX

            Vector2Int[] list =
            {
                // ligne du haut
                new(2, 2),
                new(1, 2),
                new(0, 2),
                new(-1, 2),
                new(-2, 2),

                // ligne du bas
                new(2, -2),
                new(1, -2),
                new(0, -2),
                new(-1, -2),
                new(-2, -2),

                // colonne de gauche
                new(-2, 1),
                new(-2, 0),
                new(-2, -1),

                // colonne de droite
                new(2, 1),
                new(2, 0),
                new(2, -1),

                // lui-même
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

        public float[] GetPassiveEffect(NetworkPlayer player, bool recursive = true)
        {
            var effects = new List<float>();
            for (var i = 0; i < player.Lives.Length; i++)
            {
                var e = i + NetworkPlayer.Players.FindIndex(p => NetworkParty.Instance.Turn == p.OwnerClientId);
                effects.Add(GetGlobalConfiguration().passiveEffect[
                    e % GetGlobalConfiguration().passiveEffect.Length
                ] * ArenaDescriptor.EffectMultiplier);
            }

            return effects.ToArray();
        }

        public float[] GetActiveEffect(bool recursive = true)
            => GetPassiveEffect(this.GetOwner(), recursive)
                .Select(e => -e)
                .ToArray();


        public static async UniTask<JumperCard> SpawnType()
        {
            var go = await Addressables.LoadAssetAsync<GameObject>("cards/" + GetTypeName());
            var instance = Instantiate(go);
            return instance.GetComponent<JumperCard>();
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

        public Vector3 GetSelectionOffset() => Vector3.zero;

        public void OnDestroy()
        {
            Debug.Log($"Destroying {this}");
            if (TryGetVisualCard(out var cardVisual))
                Destroy(cardVisual.gameObject);
        }
    }
}