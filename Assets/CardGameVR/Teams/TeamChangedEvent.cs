using System;
using CardGameVR.Players;
using UnityEngine.Events;

namespace CardGameVR.Teams
{
    public class TeamChangedEvent : UnityEvent<TeamChangedArgs>
    {
    }

    public class TeamChangedArgs
    {
        public PlayerController Player;
        public Team OldTeam;
        public Team NewTeam;
    }
}