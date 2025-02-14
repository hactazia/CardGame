using UnityEngine;

namespace CardGameVR.Cards.Visual
{
    public class VisualCardHandler : MonoBehaviour
    {
        public static VisualCardHandler Instance { get; private set; }

        private void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
    }
}