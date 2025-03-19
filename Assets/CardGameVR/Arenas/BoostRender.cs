using CardGameVR.Interactions;
using CardGameVR.Parties;
using CardGameVR.Players;
using UnityEngine;
using UnityEngine.UI;

namespace CardGameVR.Arenas
{
    [RequireComponent(typeof(VerticalLayoutGroup))]
    public class BoostRender : MonoBehaviour
    {
        public ArenaPlacement placement;
        public GameObject boostPrefab;
        public Button boostButton;

        public void OnSelect()
        {
            if (!boostButton.interactable) return;

            if (!placement.hand.selectedSlot && ArenaDescriptor.Instance.board.selected?.Card != null)
            {
                var card = ArenaDescriptor.Instance.board.selected.Card;
                placement.hand.selectedSlot = null;
                ArenaDescriptor.Instance.board.selected = null;
                placement.Player.BoostCard(card.GetId());
            }
        }

        private void UpdateInteractable()
        {
            boostButton.interactable = placement.Player && placement.Player.IsMyTurn && placement.Player.IsLocalPlayer;
        }

        void Start()
        {
            NetworkPlayer.OnBoosts.AddListener(player_OnBoosts);
            NetworkParty.OnTurn.AddListener(party_OnTurn);
            NetworkParty.OnState.AddListener(party_OnState);
            boostButton.onClick.AddListener(OnSelect);
            UpdateInteractable();
        }

        void OnDestroy()
        {
            NetworkPlayer.OnBoosts.RemoveListener(player_OnBoosts);
            NetworkParty.OnTurn.RemoveListener(party_OnTurn);
            NetworkParty.OnState.RemoveListener(party_OnState);
            boostButton.onClick.RemoveListener(OnSelect);
        }

        private void party_OnState(bool started)
        {
            UpdateInteractable();
            if (!started)
                foreach (Transform child in transform)
                    Destroy(child.gameObject);
            else if (placement.Player)
                player_OnBoosts(placement.Player, placement.Player.Boosts);
        }

        private void party_OnTurn(NetworkPlayer player)
        {
            UpdateInteractable();
        }


        private void player_OnBoosts(NetworkPlayer player, int boosts)
        {
            if (player != placement.Player) return;
            foreach (Transform child in transform)
                Destroy(child.gameObject);
            for (var i = 0; i < player.Boosts; i++)
                Instantiate(boostPrefab, transform);
        }
    }
}