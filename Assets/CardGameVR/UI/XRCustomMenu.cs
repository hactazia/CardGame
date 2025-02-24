using System;
using System.Globalization;
using System.Linq;
using CardGameVR.API;
using CardGameVR.Languages;
using CardGameVR.Lobbies;
using CardGameVR.Multiplayer;
using Cysharp.Threading.Tasks;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

namespace CardGameVR.UI
{
    public class XRCustomMenu : CustomMenu
    {
        [Header("Extra Pages")]
        public GameObject joinByCode;
        public GameObject main;
        
        public override void Show(bool active, string value)
        {
            Debug.Log($"CustomMenu.Show: {active} - {value}");
            gameObject.SetActive(active);
            if (!active)
            {
                CloseJoin();
                return;
            }

            SetupCreate();

            var args = ISubMenu.GetArgList(value).ToList();

            if (args.Contains("m:create"))
                ShowCreate();
            else if (args.Contains("m:join-code"))
                ShowJoinByCode();
            else if (args.Contains("m:join"))
                ShowJoin();
            else ShowMain();

            if (args.Contains("m:modal"))
            {
                var index = args.IndexOf("m:modal");
                textModalTitle.UpdateText(new[] { args[index + 1] });
                textModalMessage.UpdateText(new[] { args[index + 2] });
                modal.SetActive(true);
                ForceUpdateLayout.UpdateManually(modal);
            }
            else modal.SetActive(false);
        }

        public override void ShowJoin()
        {
            base.ShowJoin();
            joinByCode.SetActive(false);
            main.SetActive(false);
        }

        public override void ShowCreate()
        {
            base.ShowCreate();
            joinByCode.SetActive(false);
            main.SetActive(false);
        }

        public void ShowJoinByCode()
        {
            CloseJoin();
            create.SetActive(false);
            join.SetActive(false);
            joinByCode.SetActive(true);
            main.SetActive(false);
        }
        
        public void ShowMain()
        {
            CloseJoin();
            create.SetActive(false);
            join.SetActive(false);
            joinByCode.SetActive(false);
            main.SetActive(true);
        }
    }
}