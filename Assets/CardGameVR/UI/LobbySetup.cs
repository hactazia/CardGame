using CardGameVR.Cards.Groups;
using CardGameVR.Cards.Types;
using CardGameVR.Multiplayer;
using UnityEngine;

namespace CardGameVR.UI
{
    public class LobbySetup : MonoBehaviour, ISubMenu
    {
        public HorizontalCardGroup playerGroup;
        public GameObject playerCardPrefab;

        public void Show(bool active, string args)
        {
            gameObject.SetActive(active);

            if (!active)
            {
                MultiplayerManager.OnClientJoined.RemoveListener(OnClientJoined);
                MultiplayerManager.OnClientLeft.RemoveListener(OnClientLeft);
                MultiplayerManager.OnPlayerDataChanged.RemoveListener(OnPlayerDataChanged);
                return;
            }

            MultiplayerManager.OnClientJoined.AddListener(OnClientJoined);
            MultiplayerManager.OnClientLeft.AddListener(OnClientLeft);
            MultiplayerManager.OnPlayerDataChanged.AddListener(OnPlayerDataChanged);
            
            playerGroup.Clear();

            foreach (var clientId in MultiplayerManager.instance.PlayerData)
                OnClientJoined(new ClientJoinedArgs
                {
                    ClientId = clientId.ClientId,
                    Manager = MultiplayerManager.instance
                });
        }

        private void OnPlayerDataChanged(PlayerDataChangedArgs args)
        {
            foreach (var playerData in args.Manager.PlayerData)
                if (TryGetCard(playerData.ClientId, out var playerCard))
                    UpdatePlayer(playerCard, playerData);
        }

        private void OnClientJoined(ClientJoinedArgs args)
        {
            if (!args.Manager.TryGetPlayerData(args.ClientId, out var playerData)) return;
            if (TryGetCard(args.ClientId, out var playerCard)) return;
            var card = Instantiate(playerCardPrefab)
                .GetComponent<PlayerCard>();
            playerGroup.AddCard(card);
            UpdatePlayer(card, playerData);
        }

        private void OnClientLeft(ClientLeftArgs args)
        {
            if (!TryGetCard(args.ClientId, out var playerCard)) return;
            playerGroup.RemoveCard(playerCard);
            Destroy(playerCard);
        }

        private void UpdatePlayer(PlayerCard card, PlayerData playerData)
        {
            card.clientId = playerData.ClientId;
            card.SetPlayerName(playerData.PlayerName.ToString());
        }

        private bool TryGetCard(ulong clientId, out PlayerCard playerCard)
        {
            foreach (var card in playerGroup.Cards)
                if (card is PlayerCard p && p.clientId == clientId)
                {
                    playerCard = p;
                    return true;
                }

            playerCard = null;
            return false;
        }
    }
}