using System.Linq;
using CardGameVR.Cards;
using CardGameVR.Cards.Groups;
using CardGameVR.Cards.Slots;
using CardGameVR.Cards.Visual;
using CardGameVR.Parties;
using CardGameVR.Players;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace CardGameVR.Arenas
{
    public class HandGroup : HorizontalCardGroup
    {
        public ArenaPlacement arenaPlacement;
        public Transform originTransform;

        void Start()
        {
            NetworkParty.OnGameStarted.AddListener(party_OnGameStarted);
            NetworkPlayer.OnDrawHand.AddListener(party_OnDrawHand);
            NetworkPlayer.OnClearHand.AddListener(party_OnClearHand);
            NetworkParty.OnTurn.AddListener(player_OnTurn);
            OnSelect.AddListener(group_OnSelect);
        }

        void OnDestroy()
        {
            NetworkParty.OnGameStarted.RemoveListener(party_OnGameStarted);
            NetworkPlayer.OnDrawHand.RemoveListener(party_OnDrawHand);
            NetworkPlayer.OnClearHand.RemoveListener(party_OnClearHand);
            NetworkParty.OnTurn.RemoveListener(player_OnTurn);
            OnSelect.RemoveListener(group_OnSelect);
        }

        private void player_OnTurn(NetworkPlayer player)
        {
            foreach (var slot in slots)
            {
                slot.interactable = player.IsMyTurn && player.IsLocalPlayer && player == arenaPlacement.Player;
                slot.isSelected = false;
            }
        }

        private void party_OnDrawHand(NetworkPlayer player, HandCard card)
            => party_OnDrawHandAsync(player, card).Forget();

        private async UniTask party_OnDrawHandAsync(NetworkPlayer player, HandCard card)
        {
            if (player != arenaPlacement.Player) return;
            var slot = slots.Find(e => e.Card.GetId() == card.Id);
            if (slot)
            {
                Debug.Log($"Card {card.Id} already exists in hand");
                return;
            }

            var spawned = await card.Spawn();
            var visual = !spawned.TryGetVisualCard(out var cardVisual)
                ? spawned.SpawnVisualCard(visualCardHandler ?? VisualCardHandler.Instance)
                : cardVisual;
            visual.transform.position = originTransform.position;
            visual.transform.rotation = originTransform.rotation;
            Add(spawned);
            slot = slots.Find(e => e.Card.GetId() == card.Id);
            slot.interactable = player.IsMyTurn && player.IsLocalPlayer;
            if (!slot.interactable && slot.isSelected)
                slot.OnDeselect();
        }

        private void party_OnClearHand(NetworkPlayer player)
        {
            if (player != arenaPlacement.Player) return;
            Clear();
            player.DrawCard();
        }

        private void party_OnGameStarted()
            => party_OnGameStartedAsync().Forget();

        private async UniTask party_OnGameStartedAsync()
        {
            if (!arenaPlacement.Player || !arenaPlacement.Player.IsLocalPlayer) return;
            arenaPlacement.Player.ClearHand();
            for (var i = 1; i < ArenaDescriptor.InitialInHand; i++)
            {
                await UniTask.Delay(500);
                arenaPlacement.Player.DrawCard();
            }
        }

        public CardSlot selectedSlot;


        private void group_OnSelect(CardSlot slot, bool isSelected)
        {
            Debug.Log($"Selected: {slot.Index} {isSelected}");
            if (isSelected && !slot.isSelected)
            {
                foreach (var s in slots)
                    s.isSelected = false;
                selectedSlot = slot;
                selectedSlot.isSelected = true;
                return;
            }

            if (isSelected && slot.isSelected)
            {
                selectedSlot = null;
                slot.isSelected = false;
                return;
            }
        }

        public ICard GetSelected() => selectedSlot?.Card;
    }
}