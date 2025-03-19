using System;
using CardGameVR.Languages;
using CardGameVR.Parties;
using CardGameVR.Players;
using UnityEngine;
using UnityEngine.UI;

namespace CardGameVR.Arenas
{
    public class DeckButton : MonoBehaviour
    {
        public ArenaPlacement arenaPlacement;
        public Button drawButton;
        public TextLanguage text;

        void Start()
        {
            drawButton.onClick.AddListener(button_Clicked);
        }

        void OnDestroy()
        {
            drawButton.onClick.RemoveListener(button_Clicked);
        }

        private void Update()
        {
            drawButton.interactable = arenaPlacement.Player
                                      && arenaPlacement.Player.IsMyTurn
                                      && arenaPlacement.Player.IsLocalPlayer;
            text.UpdateText(IsPass ? "pass" : "draw");
        }


        private bool IsPass => arenaPlacement.Player && arenaPlacement.Player.Hand.Length >= ArenaDescriptor.MaxInHand;

        private void button_Clicked()
        {
            if (!arenaPlacement.Player.IsMyTurn || !arenaPlacement.Player.IsLocalPlayer) return;
            if (IsPass) NetworkParty.Instance.NextTurn();
            else arenaPlacement.Player.DrawCard();
        }
    }
}