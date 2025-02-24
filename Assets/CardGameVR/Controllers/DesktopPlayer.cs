using CardGameVR.Players;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CardGameVR.Controllers
{
    public class DesktopController : Controller
    {
        public Camera playerCamera;

        public float mouseSensitivity = .5f;
        public InputActionReference mouseLookAction;
        public InputActionReference toggleMenuAction;

        public override void Awake()
        {
            base.Awake();
            mouseLookAction.action.performed += OnMouseLook;
            toggleMenuAction.action.performed += OnToggleMenu;
        }

        private void OnToggleMenu(InputAction.CallbackContext ctx)
        {
            Debug.Log("Toggling menu");
            if (ctx.ReadValueAsButton())
                menu.Toggle();
        }

        private void OnMouseLook(InputAction.CallbackContext ctx)
        {
            if (Cursor.lockState != CursorLockMode.Locked) return;
            var delta = ctx.ReadValue<Vector2>();
            if (delta.magnitude < .01f) return;
            var rotation = playerCamera.transform.rotation;
            rotation *= Quaternion.Euler(-delta.y * mouseSensitivity, delta.x * mouseSensitivity, 0);

            // Clamp the rotation to prevent flipping
            var x = rotation.eulerAngles.x;
            if (x > 180) x -= 360;
            x = Mathf.Clamp(x, -85, 85);
            rotation = Quaternion.Euler(x, rotation.eulerAngles.y, 0);
            SetRotation(rotation);
        }

        public void OnEnable()
        {
            mouseLookAction.action.Enable();
            toggleMenuAction.action.Enable();
        }

        public void OnDisable()
        {
            mouseLookAction.action.Disable();
            toggleMenuAction.action.Disable();
        }

        public override void Recenter()
        {
            Debug.Log("Desktop Re-centering");
            if (!PlayerAnchor.Instance) return;
            Teleport(PlayerAnchor.Instance.transform);
        }

        public override Vector3 GetPosition()
            => transform.position;

        public override Quaternion GetRotation()
            => playerCamera.transform.rotation;

        protected override void SetPosition(Vector3 position)
            => transform.position = position;

        protected override void SetRotation(Quaternion rotation)
        {
            transform.rotation = Quaternion.Euler(0, rotation.eulerAngles.y, 0);
            playerCamera.transform.localRotation = Quaternion.Euler(rotation.eulerAngles.x, 0, 0);
        }

        public override bool TryGetTransform(HumanBodyBones bone, out Transform t)
        {
            if (bone == HumanBodyBones.Head)
            {
                t = playerCamera.transform;
                return true;
            }

            t = null;
            return false;
        }
    }
}