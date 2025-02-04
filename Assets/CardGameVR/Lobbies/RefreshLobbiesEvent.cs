using Unity.Services.Lobbies.Models;
using UnityEngine.Events;

namespace CardGameVR.Lobbies
{
    public class RefreshLobbiesEvent : UnityEvent<RefreshLobbiesArgs>
    {
    }

    public class RefreshLobbiesArgs
    {
        public LobbyManager Manager;
        public Lobby[] Lobbies;
    }
}