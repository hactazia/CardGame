using System;
using CardGameVR.Controllers;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace CardGameVR.Interactions
{
    public class DesktopInteract : MonoBehaviour
    {
        public DesktopController controller;
        
        public Camera Camera => controller.playerCamera;

        public Interactable CurrentHovered;
        public Interactable CurrentClicked;
        
        private void Update()
        {
            if (!controller) return;
            if (!Camera) return;

            var ray = new Ray(Camera.transform.position, Camera.transform.forward);
            if (Physics.Raycast(ray, out var hit, 100f))
            {
                var interactable = hit.collider.GetComponent<Interactable>();
                if (interactable)
                {
                    if (CurrentHovered != interactable)
                    {
                        CurrentHovered?.OnHoverExit();
                        CurrentHovered = interactable;
                        CurrentHovered.OnHoverEnter();
                    }
                    
                    if (CurrentClicked != interactable) 
                        ClearClick();

                    if (Mouse.current.leftButton.wasPressedThisFrame)
                    {
                        CurrentClicked?.OnDeselect();
                        CurrentClicked = interactable;
                        CurrentClicked.OnSelect();
                    }
                }
                else
                {
                    ClearHover();
                    ClearClick();
                }
            }
            else
            {
                ClearHover();
                ClearClick();
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame) 
                ClearClick();
        }

        private void ClearHover()
        {
            CurrentHovered?.OnHoverExit();
            CurrentHovered = null;
        }
        
        private void ClearClick()
        {
            CurrentClicked?.OnDeselect();
            CurrentClicked = null;
        }
    }
}