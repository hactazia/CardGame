using UnityEngine;

namespace CardGameVR.UI
{
    public class PauseMenu : MonoBehaviour, ISubMenu
    {
        public void Show(bool active, string args)
        {
            gameObject.SetActive(active);
        }
    }
}