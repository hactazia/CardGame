using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CardGameVR.Controllers
{
    public class DesktopInteract : MonoBehaviour
    {
        public DesktopController Controller;
        
        public Camera Camera => Controller.playerCamera;

        private void Update()
        {
            if (!Controller) return;
            if (!Camera) return;
            
            var ray = new Ray(Camera.transform.position, Camera.transform.forward);
            // make ui interaction
            if (Physics.Raycast(ray, out var hit, 100f))
            {
                var interactable = hit.collider.GetComponent<UnityEngine.UI.IClippable>();
                if (interactable != null)
                {
                    interactable.OnHoverEnter();
                    if (Mouse.current.leftButton.wasPressedThisFrame)
                        interactable.OnClick();
                }
            }
        }
    }
}