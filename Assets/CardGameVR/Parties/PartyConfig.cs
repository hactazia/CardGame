using CardGameVR.Teams;
using UnityEngine;

namespace CardGameVR.Parties
{
    [CreateAssetMenu(fileName = "PartyConfig", menuName = "CardGameVR/Parties/PartyConfig")]
    public class PartyConfig : ScriptableObject
    {
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Vector4 startingLevels;
        [SerializeField] private Team[] playTeams;

        public GameObject GetPlayerPrefab() => playerPrefab;
        public Vector4 GetStartingLevels() => startingLevels;
        public Team[] GetPlayTeams() => playTeams;
    }
}