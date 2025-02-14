using UnityEngine;

namespace CardGameVR.Cards.Visual
{
    public class VisualCard : MonoBehaviour
    {
        [Header("Card")]
        public ICard ReferenceCard;
        
        public void SetCard(ICard card, int index = 0)
        {
            ReferenceCard = card;
        }
        
        void Update()
        {
            if (ReferenceCard == null) return;
            
            SmoothFollow();
        }
        
        [Header("Follow Parameters")]
        [SerializeField] private float followSpeed = 30;
        
        private void SmoothFollow()
        {
            transform.position = Vector3.Lerp(
                transform.position, 
                ReferenceCard.GetTransform().position, 
                followSpeed * Time.deltaTime
            );
        }
    }
}