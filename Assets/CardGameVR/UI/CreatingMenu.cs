using System;
using CardGameVR.Languages;
using CardGameVR.Lobbies;
using UnityEngine;

namespace CardGameVR.UI
{
    public class CreatingMenu : MonoBehaviour, ISubMenu
    {
        public TextLanguage textMessage;
        
        public void Show(bool active, string args)
        {
            gameObject.SetActive(active);
            if (!active)
            {
                LobbyManager.OnCreatingLobby.RemoveListener(OnCreatingLobby);
                return;
            }
            LobbyManager.OnCreatingLobby.AddListener(OnCreatingLobby);
            OnCreatingLobby(new CreatingLobbyArgs { Status = CreatingLobbyStatus.Preparing });
        }

        public string textPreparing = "main_menu.creating.preparing";
        public string textRelayAllocation = "main_menu.creating.relay_allocation";
        public string textRelayJoinCode = "main_menu.creating.relay_join_code";
        public string textLobbyCreation = "main_menu.creating.lobby_creation";
        public string textStartHost = "main_menu.creating.start_host";

        private void OnCreatingLobby(CreatingLobbyArgs args)
        {
            switch (args.Status)
            {
                case CreatingLobbyStatus.Preparing:
                    textMessage.UpdateText(textPreparing);
                    break;
                case CreatingLobbyStatus.RelayAllocation:
                    textMessage.UpdateText(textRelayAllocation);
                    break;
                case CreatingLobbyStatus.RelayJoinCode:
                    textMessage.UpdateText(textRelayJoinCode);
                    break;
                case CreatingLobbyStatus.LobbyCreation:
                    textMessage.UpdateText(textLobbyCreation);
                    break;
                case CreatingLobbyStatus.StartHost:
                    textMessage.UpdateText(textStartHost);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}