using System.Globalization;
using CardGameVR.Languages;
using CardGameVR.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace CardGameVR.UI
{
    public class CustomLobbySelector : MonoBehaviour
    {
        public CustomMenu menu;
        public Lobby Lobby;

        public TextLanguage textLabel;
        public TextLanguage textPlayers;

        public GameObject privateIcon;

        public void UpdateContent()
        {
            textLabel.UpdateText(new[] { Lobby.Name });
            textPlayers.UpdateText(new[]
            {
                (Lobby.MaxPlayers - Lobby.AvailableSlots).ToString("0", CultureInfo.InvariantCulture),
                Lobby.MaxPlayers.ToString("0", CultureInfo.InvariantCulture)
            });
            privateIcon.SetActive(Lobby.IsPrivate);
        }

        public void Join()
        {
            Debug.Log("Joining lobby " + Lobby.Id);
            menu.JoinLobby(Lobby);
        }
    }
}