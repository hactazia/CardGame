using System;
using CardGameVR.Languages;
using CardGameVR.Lobbies;
using UnityEngine;

namespace CardGameVR.UI
{
    public class JoiningMenu : MonoBehaviour, ISubMenu
    {
        public TextLanguage textMessage;

        public void Show(bool active, string args)
        {
            gameObject.SetActive(active);
            if (!active)
            {
                LobbyManager.OnJoiningLobby.RemoveListener(OnJoiningLobby);
                return;
            }

            LobbyManager.OnJoiningLobby.AddListener(OnJoiningLobby);
            OnJoiningLobby(new JoiningLobbyArgs { Status = JoiningLobbyStatus.Preparing });
        }

        public string textPreparing = "main_menu.joining.preparing";
        public string textJoinAllocation = "main_menu.joining.join_allocation";
        public string textStartClient = "main_menu.joining.start_client";

        private void OnJoiningLobby(JoiningLobbyArgs args)
        {
            switch (args.Status)
            {
                case JoiningLobbyStatus.Preparing:
                    textMessage.UpdateText(textPreparing);
                    break;
                case JoiningLobbyStatus.JoinAllocation:
                    textMessage.UpdateText(textJoinAllocation);
                    break;
                case JoiningLobbyStatus.StartClient:
                    textMessage.UpdateText(textStartClient);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}