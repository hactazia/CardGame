﻿using UnityEngine.Events;

namespace CardGameVR.Multiplayer
{
    public class DisconnectEvent : UnityEvent<DisconnectArgs>
    {
    }
    
    public class DisconnectArgs
    {
        public string Reason;
    }
}