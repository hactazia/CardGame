using CardGameVR.Languages;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace CardGameVR.UI
{
    public class Menu : MonoBehaviour
    {
        public string defaultAction = "main";
        public MenuAction[] actions;

        // On Click events for the main menu buttons
        public void OnClick(string n) => OnClick(n, null);

        public void OnClick(string n, string value)
        {
            var action = actions.FirstOrDefault(a => a.name == n);
            if (action == null) return;
            switch (action.type)
            {
                case MenuAction.ActionType.OpenURL:
                    OpenURL(action.value);
                    break;
                case MenuAction.ActionType.OpenMenu:
                    OpenMenu(action.name, value ?? action.value);
                    break;
            }
        }

        public void OnPointerEnter(string menu)
        {
            Debug.Log("Pointer Enter: " + menu);
        }

        private static LanguagePack _pack;

        private void Awake()
        {
            if (_pack) return;
            _pack = Resources.Load<LanguagePack>("main_menu");
            LanguageManager.AddPack(_pack);
        }

        public void OnApplicationQuit()
        {
            if (!_pack) return;
            LanguageManager.RemovePack(_pack);
        }

        public void Start()
        {
            OpenMenu(defaultAction);
        }

        public void OpenURL(string url)
        {
            Application.OpenURL(url);
        }

        public void OpenMenu(string selectAction, string value = null)
        {
            Debug.Log("OpenMenu: " + selectAction);
            foreach (var action in actions)
                if (action.type == MenuAction.ActionType.OpenMenu)
                    action.targetSubMenu.Show(action.name == selectAction, value ?? action.value);
        }

        public void Close()
        {
            Debug.Log("Close Menu");
            gameObject.SetActive(false);
        }
    }

    [System.Serializable]
    public class MenuAction
    {
        public string name;
        public ActionType type;

        public string value;
        public GameObject target;
        public ISubMenu targetSubMenu => target.GetComponent<ISubMenu>();

        public enum ActionType
        {
            OpenURL,
            OpenMenu
        }
    }
}