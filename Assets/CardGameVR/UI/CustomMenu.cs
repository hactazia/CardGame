using System;
using System.Globalization;
using System.Linq;
using CardGameVR.API;
using CardGameVR.Languages;
using CardGameVR.Lobbies;
using CardGameVR.Multiplayer;
using Cysharp.Threading.Tasks;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

namespace CardGameVR.UI
{
    public class CustomMenu : MonoBehaviour, ISubMenu
    {
        public Menu menu;

        [Header("Pages")] public GameObject create;
        public GameObject join;

        [Header("Lobby list")] public GameObject lobbyList;
        public GameObject lobbyItemPrefab;
        public GameObject noLobby;
        public GameObject lobbyContent;

        [Header("Localization")] public string keyUpdateIn = "main_menu.custom.join.list_update";
        public string keyUpdating = "main_menu.custom.join.list_updating";
        public TextLanguage textUpdate;

        [Header("Create")] public TMPro.TMP_InputField inputLabel;
        public Toggle TogglePrivate;
        public string labelPatternKey = "main_menu.custom.create.label_pattern";


        public void Show(bool active, string value)
        {
            gameObject.SetActive(active);
            if (!active)
            {
                CloseJoin();
                return;
            }

            SetupCreate();

            if (value == "create")
                ShowCreate();
            else ShowJoin();
        }

        public void ShowCreate()
        {
            CloseJoin();
            create.SetActive(true);
            join.SetActive(false);
        }

        private void SetupCreate()
        {
            inputLabel.text =
                LanguageManager.Get(labelPatternKey, new object[] { MultiplayerManager.instance.playerName });
            TogglePrivate.isOn = false;
        }

        private void CloseJoin()
        {
            LobbyManager.instance.autoRefresh = false;
            LobbyManager.OnRefreshLobbies.RemoveListener(OnRefreshLobbies);
        }

        public void ShowJoin()
        {
            LobbyManager.instance.autoRefresh = true;
            LobbyManager.instance.LastRefresh = DateTime.MinValue;
            LobbyManager.OnRefreshLobbies.AddListener(OnRefreshLobbies);
            create.SetActive(false);
            join.SetActive(true);
        }

        private void OnRefreshLobbies(RefreshLobbiesArgs lobbies)
        {
            Debug.Log($"Lobbies refreshed {lobbies.Lobbies.Length}");
            var lobbySelector = lobbyContent
                .GetComponentsInChildren<CustomLobbySelector>()
                .ToList();

            if (lobbies.Lobbies.Length == 0)
            {
                noLobby.SetActive(true);
                lobbyList.SetActive(false);
                return;
            }

            noLobby.SetActive(false);
            lobbyList.SetActive(true);

            // Remove extra lobby items
            for (var i = lobbies.Lobbies.Length; i < lobbySelector.Count; i++)
                Destroy(lobbySelector[i].gameObject);

            // Add missing lobby items
            for (var i = lobbySelector.Count; i < lobbies.Lobbies.Length; i++)
            {
                var lobbyItem = Instantiate(lobbyItemPrefab, lobbyContent.transform);
                lobbySelector.Add(lobbyItem.GetComponent<CustomLobbySelector>());
            }

            // Update lobby items
            for (var i = 0; i < lobbies.Lobbies.Length; i++)
            {
                lobbySelector[i].menu = this;
                lobbySelector[i].Lobby = lobbies.Lobbies[i];
                lobbySelector[i].UpdateContent();
            }
        }

        public void Update()
        {
            if (!LobbyManager.instance) return;
            if (LobbyManager.instance.LastRefresh == DateTime.MaxValue)
                textUpdate.UpdateText(keyUpdating);
            else
                textUpdate.UpdateText(keyUpdateIn, new[]
                {
                    (LobbyManager.RefreshDelay - (DateTime.Now - LobbyManager.instance.LastRefresh).Seconds)
                    .ToString("0", CultureInfo.CurrentCulture)
                });
        }

        public void StartLobby() => StartLobbyAsync().Forget();

        private async UniTask StartLobbyAsync()
        {
            var label = inputLabel.text;
            var isPrivate = TogglePrivate.isOn;
            menu.OnClick("create");
            var lobby = await LobbyManager.instance.CreateLobby(label, isPrivate);
            if (lobby == null) return;
            Debug.Log($"Created lobby {lobby}");
            menu.OnClick("lobby");
        }

        public void JoinLobby(Lobby lobby) => JoinLobbyAsync(lobby).Forget();

        private async UniTask JoinLobbyAsync(Lobby lobby)
        {
            menu.OnClick("join");
            var o = await LobbyManager.instance.JoinLobby(lobby);
            if (o == null) return;
            Debug.Log($"Joined lobby {o}");
            menu.OnClick("lobby");
        }
    }
}