using System;
using System.Collections.Generic;
using CardGameVR.Cards.Groups;
using CardGameVR.Cards.Slots;
using CardGameVR.Parties;
using CardGameVR.Players;
using System.Linq;
using CardGameVR.Cards;
using UnityEngine;
using UnityEngine.Serialization;

namespace CardGameVR.Arenas
{
    public class BoardGroup : GridCardGroup
    {
        private static readonly int HighlightIndex = Animator.StringToHash("Highlight");
        public static int Width => ArenaDescriptor.Instance.board.gridDimension.x;
        public static int Height => ArenaDescriptor.Instance.board.gridDimension.y;

        public static bool TryGetCard(int position, out ICard card)
        {
            var slot = ArenaDescriptor.Instance.board.GetSlot(position);
            if (!slot)
            {
                card = null;
                return false;
            }

            card = slot.Card;
            return card != null;
        }


        public static bool IsInBounds(Vector2Int cell)
            => cell.x >= 0 && cell.x < Width && cell.y >= 0 && cell.y < Height;

        public static bool IsInBounds(int index)
            => index >= 0 && index < Width * Height;

        public static Vector2Int GetCell(int index)
            => new Vector2Int(index % Width, index / Width);

        public static int GetIndex(Vector2Int cell)
            => cell.y * Width + cell.x;

        public override void Start()
        {
            base.Start();
            NetworkParty.OnGameStarted.AddListener(party_OnGameStarted);
            NetworkParty.OnTurn.AddListener(party_OnTurn);
            NetworkParty.OnBoardCleared.AddListener(party_OnBoardCleared);
            NetworkParty.OnBoardCardPlaced.AddListener(party_OnBoardCardPlaced);
            NetworkParty.OnBoardCardMoved.AddListener(party_OnBoardCardMoved);
            OnSelect.AddListener(group_OnSelect);
        }

        public void OnDestroy()
        {
            NetworkParty.OnGameStarted.RemoveListener(party_OnGameStarted);
            NetworkParty.OnTurn.RemoveListener(party_OnTurn);
            NetworkParty.OnBoardCleared.RemoveListener(party_OnBoardCleared);
            NetworkParty.OnBoardCardPlaced.RemoveListener(party_OnBoardCardPlaced);
            NetworkParty.OnBoardCardMoved.RemoveListener(party_OnBoardCardMoved);
            OnSelect.RemoveListener(group_OnSelect);
        }

        private void party_OnGameStarted()
        {
            if (!NetworkPlayer.LocalPlayer.IsServer) return;
            NetworkParty.Instance.ClearBoard();
        }

        private void party_OnTurn(NetworkPlayer player)
        {
            Debug.Log($"Turn: {player.OwnerClientId}");
            RemoveHighlight();
            var turn = player.IsLocalPlayer && player.IsMyTurn;
            foreach (var slot in GetSlots())
                slot.interactable = turn;

            if (!NetworkPlayer.LocalPlayer.IsServer)
            {
                Debug.Log("You are not the server");
                return;
            }

            foreach (var p in NetworkPlayer.Players)
            {
                Debug.Log("Calculating effects on " + p.OwnerClientId);

                var lives = p.Lives;
                var activeCards = GetCards().Where(e => e.GetOwner().OwnerClientId == p.OwnerClientId).ToArray();
                foreach (var card in activeCards)
                {
                    var li = card.GetActiveEffect(p);
                    for (var i = 0; i < Math.Min(lives.Length, li.Length); i++)
                        lives[i] += li[i];
                }

                var passiveCards = GetCards().Where(e => e.GetOwner().OwnerClientId != p.OwnerClientId).ToArray();
                foreach (var card in passiveCards)
                {
                    var li = card.GetPassiveEffect(p);
                    for (var i = 0; i < Math.Min(lives.Length, li.Length); i++)
                        lives[i] += li[i];
                }

                p.Lives = lives;
            }
        }

        private void party_OnBoardCleared() => Clear();

        public CardSlot GetSelectedSlots()
            => slots.ToList().Find(e => e.isSelected);

        [FormerlySerializedAs("Selected")] public CardSlot selected;

        private void group_OnSelect(CardSlot slot, bool isSelected)
        {
            Debug.Log($"Selected: {slot.Index} {isSelected}");
            var localPlayer = NetworkPlayer.LocalPlayer;
            if (!localPlayer.IsLocalPlayer || !localPlayer.IsMyTurn) return;
            var placement = ArenaPlacement.Get(localPlayer);
            var se = placement.hand.GetSelected();

            if (isSelected && se != null)
            {
                Debug.Log($"Place card: {se.GetId()} to {slot.Index}");
                localPlayer.PlaceCard(se, slot);
                NetworkParty.Instance.NextTurn();
                return;
            }

            if (isSelected && selected && selected.Card != null)
            {
                RemoveHighlight();

                if (selected == slot)
                {
                    selected.isSelected = false;
                    selected = null;
                    return;
                }

                var moves = selected.Card.CanMoveTo();
                if (!moves.Contains(slot.Index)) return;

                localPlayer.MoveCard(selected, slot);
                NetworkParty.Instance.NextTurn();

                selected.isSelected = false;
                selected = null;

                return;
            }

            if (isSelected && slot.Card != null)
            {
                PrintHighlight(slot);
                selected = slot;
                selected.isSelected = true;
                foreach (var s in placement.hand.GetSlots())
                    s.isSelected = false;
                placement.hand.selectedSlot = null;
            }
        }

        private void RemoveHighlight()
        {
            Debug.Log($"Remove highlight");
            foreach (var s in slots)
                s.animator?.SetInteger(HighlightIndex, 0);
        }

        private void PrintHighlight(CardSlot slot)
        {
            Debug.Log($"Highlight: {slot.Index} {slot.Card}");
            var card = slot.Card;
            var indexes = card.CanMoveTo();
            foreach (var index in indexes)
            {
                var s = GetSlot(index);
                if (!s) continue;
                if (s.Card == null) // empty slot
                    s.animator?.SetInteger(HighlightIndex, 1);
                else if (s == slot)
                    s.animator?.SetInteger(HighlightIndex, 2);
                else if (s.Card.GetOwner() == card.GetOwner())
                    s.animator?.SetInteger(HighlightIndex, 3);
                else s.animator?.SetInteger(HighlightIndex, 4);
            }
        }


        private void party_OnBoardCardPlaced(NetworkPlayer player, GridCard boardCard)
        {
            var hand = ArenaPlacement.Get(player).hand;
            var handCard = Array.Find(hand.GetCards(), e => e.GetId() == boardCard.Id);
            Debug.Log($"Place card: {boardCard.Id} with {handCard}");
            var slot = GetSlot(boardCard.Index);
            Set(slot, handCard);
            hand.Remove(handCard);
        }

        private void party_OnBoardCardMoved(NetworkPlayer player, GridCard boardCard, int from)
        {
            var fromSlot = GetSlot(from);
            var toSlot = GetSlot(boardCard.Index);
            Debug.Log($"Moved from {boardCard.Id} from {fromSlot} to {toSlot}");
            toSlot.Card?.Destroy();
            toSlot.SetCard(null);
            var card = fromSlot.Card;
            fromSlot.SetCard(null);
            Set(toSlot, card);
        }
    }
}