using CardGameVR.Languages;
using UnityEngine;
using UnityEngine.Serialization;

namespace CardGameVR.Cards.Types
{
    public class PlayerCard : MonoBehaviour, ICard
    {
        public Transform GetTransform() => transform;
        [HideInInspector] public ulong clientId;
        public TextLanguage textPlayerName;

        public void SetPlayerName(string playerName)
            => textPlayerName.UpdateText(new[] { playerName });
    }
}