using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CardGameVR.Interactions
{
    public class SelectableInteractable : Interactable
    {
        public Selectable selectable;

        public override void OnHoverEnter()
            => selectable.OnPointerEnter(new PointerEventData(EventSystem.current));

        public override void OnHoverExit()
            => selectable.OnPointerExit(new PointerEventData(EventSystem.current));

        public override void OnSelect()
        {
            selectable.OnPointerDown(new PointerEventData(EventSystem.current));
            if (selectable is Toggle toggle)
                toggle.isOn = !toggle.isOn;
            else if (selectable is Button button)
                button.OnPointerClick(new PointerEventData(EventSystem.current));
        }

        public override void OnDeselect()
            => selectable.OnPointerUp(new PointerEventData(EventSystem.current));
    }
}