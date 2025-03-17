using CardGameVR.Players;
using UnityEngine;
using System.Linq;
using CardGameVR.Parties;

namespace CardGameVR.Arenas
{
    public class ReadyStart : MonoBehaviour
    {
        void Start()
        {
            NetworkPlayer.OnReady.AddListener(player_OnReady);
        }
        
        void OnDestroy()
        {
            NetworkPlayer.OnReady.RemoveListener(player_OnReady);
        }

        public int[] CheckReady() => new[]
        {
            NetworkPlayer.Players.Count(player => player.Ready),
            NetworkPlayer.Players.Count,
            ArenaDescriptor.MinPlayers,
            ArenaDescriptor.MaxPlayers
        };
        
        private void player_OnReady(NetworkPlayer player, bool ready)
        {
            if (!NetworkPlayer.LocalPlayer.IsServer) return;
            Debug.Log($"Player {player.OwnerClientId} is ready: {ready}");
            if (NetworkParty.Instance.IsStarted) return;
            var reads = CheckReady();
            if (reads[0] >= reads[2] && reads[0] >= reads[1])
            {
                Debug.Log("All players are ready, starting game...");
                NetworkParty.Instance.StartGame();
            }
        }
    }
}