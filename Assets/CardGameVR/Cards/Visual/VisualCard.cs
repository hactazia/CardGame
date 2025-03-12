using CardGameVR.ScriptableObjects;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace CardGameVR.Cards.Visual
{
    public class VisualCard : MonoBehaviour
    {
        private bool initalize = false;

        [Header("Card")] public ICard parentCard;
        private Transform cardTransform;
        private Vector3 rotationDelta;
        private int savedIndex;
        Vector3 movementDelta;
        private Canvas canvas;

        [Header("References")] public Transform visualShadow;
        private float shadowOffset = 20;
        private Vector2 shadowDistance;
        private Canvas shadowCanvas;
        [SerializeField] private Transform shakeParent;
        [SerializeField] private Transform tiltParent;
        [SerializeField] private Image cardImage;

        [Header("Follow Parameters")] [SerializeField]
        private float followSpeed = 30;

        [Header("Rotation Parameters")] [SerializeField]
        private float rotationAmount = 20;

        [SerializeField] private float rotationSpeed = 20;
        [SerializeField] private float autoTiltAmount = 30;
        [SerializeField] private float manualTiltAmount = 20;
        [SerializeField] private float tiltSpeed = 20;

        [Header("Scale Parameters")] [SerializeField]
        private bool scaleAnimations = true;

        [SerializeField] private float scaleOnHover = 1.15f;
        [SerializeField] private float scaleOnSelect = 1.25f;
        [SerializeField] private float scaleTransition = .15f;
        [SerializeField] private Ease scaleEase = Ease.OutBack;

        [Header("Select Parameters")] [SerializeField]
        private float selectPunchAmount = 20;

        [Header("Hober Parameters")] [SerializeField]
        private float hoverPunchAngle = 5;

        [SerializeField] private float hoverTransition = .15f;

        [Header("Swap Parameters")] [SerializeField]
        private bool swapAnimations = true;

        [SerializeField] private float swapRotationAngle = 30;
        [SerializeField] private float swapTransition = .15f;
        [SerializeField] private int swapVibrato = 5;

        [Header("Curve")] [SerializeField] private bool useCurve = true;
        [SerializeField] private CurveParameters curve;

        private float curveYOffset;
        private float curveRotationOffset;
        private Coroutine pressCoroutine;

        private void Start()
        {
            shadowDistance = visualShadow.localPosition;
        }

        public void Initialize(ICard target)
        {
            //Declarations
            parentCard = target;
            cardTransform = target.GetTransform();
            canvas = GetComponent<Canvas>();
            shadowCanvas = visualShadow.GetComponent<Canvas>();

            //Event Listening
            parentCard.PointerEnterEvent.AddListener(PointerEnter);
            parentCard.PointerExitEvent.AddListener(PointerExit);
            parentCard.BeginDragEvent.AddListener(BeginDrag);
            parentCard.EndDragEvent.AddListener(EndDrag);
            parentCard.PointerDownEvent.AddListener(PointerDown);
            parentCard.PointerUpEvent.AddListener(PointerUp);
            parentCard.SelectEvent.AddListener(Select);

            //Initialization
            initalize = true;
        }

        public void UpdateIndex()
        {
            transform.SetSiblingIndex(parentCard.GetTransform().parent.GetSiblingIndex());
        }

        void Update()
        {
            if (!initalize || parentCard == null) return;

            HandPositioning();
            SmoothFollow();
            FollowRotation();
            CardTilt();
        }

        private void HandPositioning()
        {
            if (!useCurve) return;
            curveYOffset = curve.positioning.Evaluate(parentCard.GroupIndex())
                           * curve.positioningInfluence
                           * parentCard.GroupCount();
            curveYOffset = parentCard.GroupCount() < 5 ? 0 : curveYOffset;
            curveRotationOffset = curve.rotation.Evaluate(parentCard.GroupIndex());
        }

        private void CardTilt()
        {
            savedIndex = parentCard.IsDragging() ? savedIndex : parentCard.GroupIndex();
            var sine = Mathf.Sin(Time.time + savedIndex) * (parentCard.IsHovering() ? .2f : 1);
            var cosine = Mathf.Cos(Time.time + savedIndex) * (parentCard.IsHovering() ? .2f : 1);

            var offset = transform.position - Camera.main.ScreenToWorldPoint(InputSystem.GetDevice<Mouse>().position.ReadValue());
            var tiltX = parentCard.IsDragging() ? -offset.y * manualTiltAmount : 0;
            var tiltY = parentCard.IsDragging() ? offset.x * manualTiltAmount : 0;
            var tiltZ = parentCard.IsDragging()
                ? tiltParent.eulerAngles.z
                : useCurve
                    ? curveRotationOffset * (curve.rotationInfluence * parentCard.GroupCount())
                    : 0;

            var lerpX = Mathf.LerpAngle(tiltParent.eulerAngles.x, tiltX + sine * autoTiltAmount,
                tiltSpeed * Time.deltaTime);
            var lerpY = Mathf.LerpAngle(tiltParent.eulerAngles.y, tiltY + cosine * autoTiltAmount,
                tiltSpeed * Time.deltaTime);
            var lerpZ = Mathf.LerpAngle(tiltParent.eulerAngles.z, tiltZ, tiltSpeed / 2 * Time.deltaTime);

            tiltParent.eulerAngles = new Vector3(lerpX, lerpY, lerpZ);
        }

        private void SmoothFollow()
            => transform.position = Vector3.Lerp(
                transform.position,
                cardTransform.position + Vector3.up * (parentCard.IsDragging() ? 0 : curveYOffset),
                followSpeed * Time.deltaTime
            );

        private void FollowRotation()
        {
            var movement = transform.position - cardTransform.position;
            movementDelta = Vector3.Lerp(movementDelta, movement, 25 * Time.deltaTime);
            var movementRotation = (parentCard.IsDragging() ? movementDelta : movement) * rotationAmount;
            rotationDelta = Vector3.Lerp(rotationDelta, movementRotation, rotationSpeed * Time.deltaTime);
            transform.eulerAngles = new Vector3(
                transform.eulerAngles.x,
                transform.eulerAngles.y,
                Mathf.Clamp(rotationDelta.x, -60, 60)
            );
        }

        private void Select(ICard card, bool state)
        {
            DOTween.Kill(2, true);
            float dir = state ? 1 : 0;
            shakeParent.DOPunchPosition(shakeParent.up * selectPunchAmount * dir, scaleTransition);
            shakeParent.DOPunchRotation(Vector3.forward * (hoverPunchAngle / 2), hoverTransition, 20).SetId(2);

            if (scaleAnimations)
                transform.DOScale(scaleOnHover, scaleTransition).SetEase(scaleEase);
        }

        public void Swap(float dir = 1)
        {
            if (!swapAnimations)
                return;

            DOTween.Kill(2, true);
            shakeParent.DOPunchRotation(Vector3.forward * swapRotationAngle * dir, swapTransition, swapVibrato)
                .SetId(3);
        }

        private void BeginDrag(ICard card)
        {
            if (scaleAnimations)
                transform.DOScale(scaleOnSelect, scaleTransition).SetEase(scaleEase);

            canvas.overrideSorting = true;
        }

        private void EndDrag(ICard card)
        {
            canvas.overrideSorting = false;
            transform.DOScale(1, scaleTransition).SetEase(scaleEase);
        }

        private void PointerEnter(ICard card)
        {
            if (scaleAnimations)
                transform.DOScale(scaleOnHover, scaleTransition).SetEase(scaleEase);

            DOTween.Kill(2, true);
            shakeParent.DOPunchRotation(Vector3.forward * hoverPunchAngle, hoverTransition, 20).SetId(2);
        }

        private void PointerExit(ICard card)
        {
            if (!parentCard.WasDragged())
                transform.DOScale(1, scaleTransition).SetEase(scaleEase);
        }

        private void PointerUp(ICard card, bool longPress)
        {
            if (scaleAnimations)
                transform.DOScale(longPress ? scaleOnHover : scaleOnSelect, scaleTransition).SetEase(scaleEase);
            canvas.overrideSorting = false;

            visualShadow.localPosition = shadowDistance;
            shadowCanvas.overrideSorting = true;
        }

        private void PointerDown(ICard card)
        {
            if (scaleAnimations)
                transform.DOScale(scaleOnSelect, scaleTransition).SetEase(scaleEase);

            visualShadow.localPosition += -Vector3.up * shadowOffset;
            shadowCanvas.overrideSorting = false;
        }

        private void OnDestroy()
        {
            parentCard.PointerEnterEvent.RemoveListener(PointerEnter);
            parentCard.PointerExitEvent.RemoveListener(PointerExit);
            parentCard.BeginDragEvent.RemoveListener(BeginDrag);
            parentCard.EndDragEvent.RemoveListener(EndDrag);
            parentCard.PointerDownEvent.RemoveListener(PointerDown);
            parentCard.PointerUpEvent.RemoveListener(PointerUp);
            parentCard.SelectEvent.RemoveListener(Select);
            parentCard = null;
        }
    }
}