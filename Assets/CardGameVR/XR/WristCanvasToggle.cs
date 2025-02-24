using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace CardGameVR.XR
{
    public class WristCanvasToggle : MonoBehaviour
    {
        [SerializeField] private Transform targetTransform;
        [SerializeField] private Transform playerCamera;
        [SerializeField] private Transform wristCanvas;
        [SerializeField] private float showAngleThreshold = 60f;
        [SerializeField] private float hideAngleThreshold = 80f;

        void Start()
        {
            if (!wristCanvas) return;
            wristCanvas.gameObject.SetActive(false);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(targetTransform.position, playerCamera.position);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(targetTransform.position, targetTransform.position + targetTransform.forward * 0.5f);
        }

        public bool isCanvasVisible;
        public float angle = 0f;

        void Update()
        {
            if (!wristCanvas || !playerCamera)
                return;

            var canvasForward = targetTransform.forward;
            var directionToCamera = (playerCamera.position - wristCanvas.position).normalized;
            angle = Vector3.Angle(canvasForward, directionToCamera);
            switch (isCanvasVisible)
            {
                case false when angle < showAngleThreshold:
                    wristCanvas.gameObject.SetActive(true);
                    isCanvasVisible = true;
                    break;
                case true when angle > hideAngleThreshold:
                    wristCanvas.gameObject.SetActive(false);
                    isCanvasVisible = false;
                    break;
            }
        }
    }
}