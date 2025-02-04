using CardGameVR.Languages;
using UnityEngine;

namespace CardGameVR.UI
{
    public class MainMenu : MonoBehaviour, ISubMenu
    {
        public void Show(bool active, string args)
        {
            gameObject.SetActive(active);
        }
    }
}