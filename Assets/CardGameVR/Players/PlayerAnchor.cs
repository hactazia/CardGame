using UnityEngine;

namespace CardGameVR
{
    public class PlayerAnchor : MonoBehaviour
    {
        public static PlayerAnchor Instance;

        public bool isDefault;

        public void Awake()
        {
            if (!isDefault) return;
            Instance = this;
        }


        public Vector3 GetPosition()
            => transform.position;

        public Quaternion GetRotation()
            => transform.rotation;

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