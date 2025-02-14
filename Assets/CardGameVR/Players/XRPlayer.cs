using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CardGameVR.Players
{
    public class XRPlayer : Player
    {
        public Camera playerCamera;
        public InputActionReference recenterAction;
        public Transform physicalMenu;

        private void Start()
        {
            recenterAction.action.Enable();
            recenterAction.action.performed += OnPerformCenter;
            physicalMenu.SetParent(null);
            DontDestroyOnLoad(physicalMenu.gameObject);
        }

        public void OnDestroy()
        {
            recenterAction.action.Disable();
            recenterAction.action.performed -= OnPerformCenter;
            Destroy(physicalMenu.gameObject);
        }

        private void OnPerformCenter(InputAction.CallbackContext context)
            => Recenter();


        public override Vector3 GetPosition()
            => transform.position;

        public override Quaternion GetRotation()
            => playerCamera.transform.rotation;

        protected override void SetPosition(Vector3 position)
        {
            var xrOrigin = GetComponent<XROrigin>();
            xrOrigin.MoveCameraToWorldLocation(position + transform.up * xrOrigin.CameraInOriginSpaceHeight);
        }

        protected override void SetRotation(Quaternion rotation)
        {
            var xrOrigin = GetComponent<XROrigin>();
            var up = rotation * Vector3.up;
            var forward = rotation * Vector3.forward;
            xrOrigin.MatchOriginUpCameraForward(up, forward);
        }

        public override void Recenter()
        {
            Debug.Log("XR Re-centering");
            if(!PlayerAnchor.Instance) return;
            var anchor = PlayerAnchor.Instance.transform;
            Teleport(anchor);
            if (!physicalMenu) return;
            physicalMenu.position = anchor.position;
            physicalMenu.rotation = anchor.rotation;
        }

        public void OnDrawGizmos()
        {
            var xrOrigin = GetComponent<XROrigin>();
            if (!xrOrigin) return;
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(xrOrigin.CameraInOriginSpacePos, 0.1f);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(xrOrigin.OriginInCameraSpacePos, 0.1f);
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(XRPlayer))]
    public class VRPlayerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var player = (XRPlayer)target;
            if (GUILayout.Button("Recenter"))
                player.Recenter();
        }
    }
#endif
}