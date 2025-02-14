using CardGameVR.Cards.Visual;
using CardGameVR.Languages;
using UnityEngine;

namespace CardGameVR.Cards.Types
{
    public class PlayerCard : MonoBehaviour, ICard
    {
        public Transform GetTransform() => transform;

        [Header("Player")] [HideInInspector] public ulong clientId;
        public TextLanguage textPlayerName;

        public void SetPlayerName(string playerName)
            => textPlayerName.UpdateText(new[] { playerName });

        [Header("Visual")] 
        public VisualCard visualCardPrefab;
        public VisualCard visualCard;

        public VisualCard SpawnVisualCard(VisualCardHandler handler)
        {
            if (visualCard) return visualCard;
            visualCard = Instantiate(
                visualCardPrefab,
                handler.transform
            ).GetComponent<VisualCard>();
            visualCard.transform.position = transform.position;
            visualCard.transform.rotation = transform.rotation;
            visualCard.SetCard(this);
            return visualCard;
        }

        public void Destroy()
        {
            if (visualCard)
                Destroy(visualCard.gameObject);
            Destroy(gameObject);
        }
    }
}