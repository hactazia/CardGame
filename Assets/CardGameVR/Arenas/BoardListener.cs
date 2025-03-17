/*using System.Linq;
using CardGameVR.Cards;
using CardGameVR.Cards.Groups;
using CardGameVR.Cards.Slots;
using CardGameVR.Parties;
using CardGameVR.Players;
using UnityEngine;
using UnityEngine.Serialization;

namespace CardGameVR.Arenas
{
    public class BoardListener : MonoBehaviour
    {
        public ArenaPlacement ArenaPlacement;
        public HandListener HandListener;

        public CardSlot SelectedCard;


        public void Start()
        {
            ArenaDescriptor.Instance.gameBoard.OnSlotAdded.AddListener(OnSlotUpdate);
        }

        public void OnDestroy()
        {
            ArenaDescriptor.Instance.gameBoard.OnSlotAdded.RemoveListener(OnSlotUpdate);
            foreach (var slot in ArenaDescriptor.Instance.gameBoard.GetSlots())
                slot.onSelect.RemoveListener(OnCardSelect);
        }

        public void OnSlotUpdates()
        {
            foreach (var slot in ArenaDescriptor.Instance.gameBoard.GetSlots())
                OnSlotUpdate(slot);
        }


        private void OnSlotUpdate(CardSlot slot)
        {
            slot.interactable = ArenaPlacement.IsYourTurn;
            if (slot.interactable)
                slot.onSelect.AddListener(OnCardSelect);
            else slot.onSelect.RemoveListener(OnCardSelect);
        }

        private void OnCardSelect(CardSlot slot, bool isSelected)
        {
            if (!ArenaPlacement.IsYourTurn) return;

            if (isSelected && slot != SelectedCard)
            {
                // interaction place card on board
                if (slot.Card == null && HandListener.SelectedCard != null)
                {
                    Debug.Log($"Place card: {HandListener.SelectedCard.Card.GetId()} to {slot.Index}");
                    ArenaPlacement.NetworkPlayer.PlaceCard(HandListener.SelectedCard.Card.GetId(), slot.Index);
                    NetworkParty.Instance.NextTurn();
                    return;
                }

                // interaction move card on board
                if (slot.Card == null && SelectedCard?.Card != null)
                {
                    Debug.Log($"Move card: {SelectedCard.Card.GetId()} to {slot.Index}");
                    NetworkParty.Instance.MoveCard(SelectedCard.Index, slot.Index, ArenaPlacement.NetworkPlayer.OwnerClientId);
                    NetworkParty.Instance.NextTurn();
                    return;
                }

                if (slot.Card != null)
                    Select(slot);

                return;
            }

            if (!isSelected && slot == SelectedCard)
                Deselect();


            // if (HandListener.SelectedCard != null && slot.Card == null)
            // {
            //     var card = HandListener.SelectedCard.Card;
            //     HandListener.Deselect();
// 
            //     foreach (var s in ArenaDescriptor.Instance.gameBoard.GetSlots())
            //         s.OnDeselect();
            //     Deselect();
// 
            //     ArenaPlacement.Player.PlaceCard(card.GetId(), slot.Index);
// 
            //     Debug.Log($"Place card: {card}");
// 
            //     return;
            // }
// 
            // Debug.Log($"{slot != null} {slot?.Card} {isSelected} {SelectedCard != null} {SelectedCard?.Card}");
            // 
            // if (slot == SelectedCard && !isSelected)
            // {
            //     Deselect();
            //     return;
            // }
// 
            // if (HandListener.SelectedCard == null && SelectedCard == null && slot.Card != null && isSelected)
            // {
            //     Select(slot);
            //     HandListener.Deselect();
            //     return;
            // }
// 
            // if (HandListener.SelectedCard == null && SelectedCard != null && slot.Card == null)
            // {
            //     var card = SelectedCard.Card;
            //     Deselect();
            //     HandListener.Deselect();
// 
            //     Debug.Log($"Move card: {card.GetId()} to {SelectedCard.Index} > {slot.Index}");
            //     return;
            // }
        }

        public void Deselect()
        {
            if (!SelectedCard) return;
            Debug.Log($"Deselect board: {SelectedCard.Card}");
            SelectedCard = null;
        }


        public void Select(CardSlot card)
        {
            foreach (var s in ArenaPlacement.handCardGroup
                         .GetSlots()
                         .Where(s => s.isSelected && s != card))
                s.OnDeselect();
            Debug.Log($"Selected board card: {card.Card}");
            SelectedCard = card;
        }
    }
}*/