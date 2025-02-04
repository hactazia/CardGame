using CardGameVR.Teams;
using UnityEngine;

namespace CardGameVR.Multiplayer
{
    [CreateAssetMenu(fileName = "MultiplayerConfig", menuName = "CardGameVR/Parties/MultiplayerConfig")]
    public class MultiplayerConfig : ScriptableObject
    {
        [SerializeField] private Vector2Int playerCountRange;
        [SerializeField] private Team[] playTeams;
        
        public int GetMinPlayerCount() => playerCountRange.x;
        public int GetMaxPlayerCount() => playerCountRange.y;
        public Team[] GetPlayTeams() => playTeams;
    }
}