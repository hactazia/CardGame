using System;
using CardGameVR.Parties;
using CardGameVR.Players;
using UnityEngine;

namespace CardGameVR.Arenas
{
    public class PlayerEventListener
    {
        public readonly ulong ClientId;
        private GridCard _lastGridCard;
        private PlayerHandCard _lastPlayerHandCard;
        
        public PlayerEventListener(ulong clientId)
        {
            Debug.Log($"Listening to player {clientId}");
            ClientId = clientId;
        }

        public void OnBoardCardAdded(GridCard arg0)
        {
            Debug.Log($"OnBoardCardAdded: {arg0.Id}");
            _lastGridCard = arg0;
            OnActionUpdate();
        }

        public void OnPlayerCardRemoved(PlayerHandCard arg0)
        {
            Debug.Log($"OnPlayerCardRemoved: {arg0.Id}");
            _lastPlayerHandCard = arg0;
            OnActionUpdate();
        }

        public void OnActionUpdate()
        {
            if (_lastGridCard.CardType != _lastPlayerHandCard.CardType) return;
            if (_lastGridCard.Id != _lastPlayerHandCard.Id) return;
            var player = PlayerNetwork.GetPlayer(ClientId);
            if (!player) return;
            var card = Array.Find(player.GetHandCards(), c => c.GetId() == _lastPlayerHandCard.Id);
            if (card == null)
            {
                Debug.LogWarning($"Card {_lastPlayerHandCard.Id} not found in the hand of player {ClientId}");
                return;
            }
            
            var slot = NetworkParty.Instance.GetBoardSlot(_lastGridCard.Index);
            slot.SetCard(card);
            Debug.Log($"{card} {card.GetId()} {_lastPlayerHandCard.Id}");

            Debug.Log(
                $"La carte {_lastPlayerHandCard.CardType} dans la main du joueur {ClientId} a été placée sur la grille à la position {_lastGridCard.Index}");
        }
    }
}