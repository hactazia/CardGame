/*using System;
using System.Collections.Generic;
using CardGameVR.Parties;
using CardGameVR.Players;
using UnityEngine;

namespace CardGameVR.Parties
{
    public class PlayerEventListener
    {
        public static readonly List<PlayerEventListener> PlayerEvents = new();

        public static PlayerEventListener GetPlayerEvent(ulong clientId)
            => PlayerEvents.Find(p => p.ClientId == clientId);

        public static void AddPlayerEvent(ulong clientId)
        {
            if (PlayerEvents.Exists(p => p.ClientId == clientId)) return;
            PlayerEvents.Add(new PlayerEventListener(clientId));
        }

        public static void RemovePlayerEvent(ulong clientId)
        {
            var playerEvent = PlayerEvents.Find(p => p.ClientId == clientId);
            if (playerEvent == null) return;
            PlayerEvents.Remove(playerEvent);
        }

        public readonly ulong ClientId;
        private NetworkPlayer NetworkPlayer => NetworkPlayer.GetPlayer(ClientId);

        private GridCard _lastGridCard;
        private HandCard _lastHandCard;

        public PlayerEventListener(ulong clientId)
        {
            Debug.Log($"Listening to player {clientId}");
            ClientId = clientId;

            NetworkParty.OnBoardCardAdded.AddListener(OnBoardCardAdded);
            NetworkPlayer.onPlayerCardRemoved.AddListener(OnPlayerCardRemoved);
        }

        public void OnBoardCardAdded(GridCard arg0)
        {
            Debug.Log($"OnBoardCardAdded: {arg0.Id}");
            _lastGridCard = arg0;
            OnActionUpdate();
        }

        public void OnPlayerCardRemoved(HandCard arg0)
        {
            Debug.Log($"OnPlayerCardRemoved: {arg0.Id}");
            _lastHandCard = arg0;
            OnActionUpdate();
        }

        public void OnActionUpdate()
        {
            if (_lastGridCard.CardType != _lastHandCard.CardType) return;
            if (_lastGridCard.Id != _lastHandCard.Id) return;
            var player = NetworkPlayer.GetPlayer(ClientId);
            if (!player) return;
            var card = Array.Find(player.GetHandCards(), c => c.GetId() == _lastHandCard.Id);
            if (card == null)
            {
                Debug.LogWarning($"Card {_lastHandCard.Id} not found in the hand of player {ClientId}");
                return;
            }

            var slot = NetworkParty.Instance.GetBoardSlot(_lastGridCard.Index);
            slot.SetCard(card);
            Debug.Log($"{card} {card.GetId()} {_lastHandCard.Id}");

            Debug.Log(
                $"La carte {_lastHandCard.CardType} dans la main du joueur {ClientId} a été placée sur la grille à la position {_lastGridCard.Index}");
        }
    }
}*/