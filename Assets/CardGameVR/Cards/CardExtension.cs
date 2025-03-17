using System;
using CardGameVR.Arenas;
using CardGameVR.Parties;
using CardGameVR.Players;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CardGameVR.Cards
{
    public static class CardExtension
    {
        public static bool TryHand(this ICard card, out HandCard handCard)
        {
            foreach (var player in NetworkPlayer.Players)
            {
                var index = Array.FindIndex(player.Hand, c => c.Id == card.GetId());
                if (index == -1) continue;
                handCard = player.Hand[index];
                return true;
            }

            handCard = default;
            return false;
        }

        public static bool TryBoard(this ICard card, out GridCard boardCard)
        {
            var grid = NetworkParty.Instance.Board;
            var index = Array.FindIndex(grid, c => c.Id == card.GetId());
            if (index == -1)
            {
                boardCard = default;
                return false;
            }

            boardCard = grid[index];
            return true;
        }

        public static NetworkPlayer GetOwner(this ICard card)
        {
            if (card.TryHand(out var handCard))
                foreach (var player in NetworkPlayer.Players)
                {
                    var index = Array.FindIndex(player.Hand, c => c.Id == card.GetId());
                    if (index == -1) continue;
                    return player;
                }
            else if (card.TryBoard(out var boardCard))
                return NetworkPlayer.GetPlayer(boardCard.OwnerId);

            return null;
        }

        public static int GetIndex(this ICard card)
            => card.TryBoard(out var b) ? b.Index : -1;

        public static Vector2Int GetCell(this ICard card)
        {
            var index = card.GetIndex();
            return BoardGroup.IsInBounds(index)
                ? BoardGroup.GetCell(index)
                : new Vector2Int(-1, -1);
        }


        public static int GroupCount(this ICard card)
            => card.GetSlot()
                ? card.GetSlot().Group.SlotCount() - 1
                : 0;

        public static int GroupIndex(this ICard card)
            => card.GetSlot()
                ? card.GetSlot().Group.IndexOf(card.GetSlot())
                : 0;

        public static void Destroy(this ICard card)
        {
            if (card.TryGetVisualCard(out var cardVisual))
                Object.Destroy(cardVisual.gameObject);
            Object.Destroy(card.GetTransform().gameObject);
        }
    }
}