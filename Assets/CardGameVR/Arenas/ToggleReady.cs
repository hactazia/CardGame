using CardGameVR.Players;
using UnityEngine;
using UnityEngine.UI;

namespace CardGameVR.Arenas
{
    public class ToggleReady : MonoBehaviour
    {
        public ArenaPlacement placement;
        public Toggle toggle;

        private void Start()
        {
            toggle.onValueChanged.AddListener(toggle_OnValueChanged);
            NetworkPlayer.OnPlayerJoined.AddListener(player_OnJoined);
            NetworkPlayer.OnPlayerLeft.AddListener(player_OnLeft);
            NetworkPlayer.OnReady.AddListener(player_OnReady);
            toggle.isOn = placement.Player && placement.Player.Ready;
        }

        private void OnDestroy()
        {
            toggle.onValueChanged.RemoveListener(toggle_OnValueChanged);
            NetworkPlayer.OnPlayerJoined.RemoveListener(player_OnJoined);
            NetworkPlayer.OnPlayerLeft.RemoveListener(player_OnLeft);
            NetworkPlayer.OnReady.RemoveListener(player_OnReady);
        }

        private void toggle_OnValueChanged(bool isOn)
        {
            if (!placement.Player || !placement.Player.IsLocalPlayer) return;
            placement.Player.Ready = isOn;
        }

        private void player_OnJoined(NetworkPlayer player)
        {
            if (player != placement.Player) return;
            if (toggle.isOn == player.Ready) return;
            toggle.isOn = player.Ready;
        }

        private void player_OnLeft(NetworkPlayer player)
        {
            if (player != placement.Player) return;
            toggle.isOn = false;
        }

        private void player_OnReady(NetworkPlayer player, bool isReady)
        {
            if (player != placement.Player) return;
            toggle.isOn = player.Ready;
        }
    }
}