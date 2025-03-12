using UnityEngine.UI;

namespace CardGameVR.Interactions
{
    public class ToggleInteractable : Interactable
    {
        public Toggle toggle;

        public override void OnSelect()
            => toggle.isOn = !toggle.isOn;
    }
}