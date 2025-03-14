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
            => selectable.OnSelect(new BaseEventData(EventSystem.current));

        public override void OnDeselect()
            => selectable.OnDeselect(new BaseEventData(EventSystem.current));
    }
}