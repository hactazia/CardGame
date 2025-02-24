using System;
using UnityEngine;

namespace CardGameVR.Players
{
    public class PlayerAnchor : MonoBehaviour
    {
        public static PlayerAnchor Instance;

        public bool isDefault;

        public bool IsDefault
        {
            get => isDefault;
            set
            {
                isDefault = value;
                if (isDefault)
                    Instance = this;
            }
        }

        public void Awake()
        {
            if (!isDefault) return;
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
        
#if UNITY_EDITOR
        public void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.1f);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + transform.forward);
        }
#endif
    }
}