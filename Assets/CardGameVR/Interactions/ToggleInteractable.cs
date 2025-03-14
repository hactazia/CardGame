using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CardGameVR.Interactions
{
    public class ToggleInteractable : Interactable
    {
        public Toggle toggle;

        public override void OnHoverEnter()
            => toggle.OnPointerEnter(new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Left
            });

        public override void OnHoverExit()
            => toggle.OnPointerExit(new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Left
            });

        public override void OnSelect()
            => toggle.OnSelect(new BaseEventData(EventSystem.current)
            {
                selectedObject = toggle.gameObject
            });

        public override void OnDeselect()
            => toggle.OnDeselect(new BaseEventData(EventSystem.current));
    }
}