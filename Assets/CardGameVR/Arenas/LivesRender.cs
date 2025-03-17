using System;
using System.Collections.Generic;
using CardGameVR.Controllers;
using CardGameVR.Parties;
using UnityEngine;
using UnityEngine.UI;

namespace CardGameVR.Arenas
{
    public class LivesRender : MonoBehaviour
    {
        public ArenaPlacement arenaPlacement;
        public Transform originTransform;

        public Transform livesContainer;
        public Slider lifePrefab;
        public List<Slider> lives = new();

        private void Start()
        {
            NetworkParty.OnGameStarted.AddListener(party_OnGameStarted);
        }

        private void OnDestroy()
        {
            NetworkParty.OnGameStarted.RemoveListener(party_OnGameStarted);
        }

        public void LateUpdate()
        {
            if (arenaPlacement.Player
                && !arenaPlacement.Player.IsLocalPlayer
                && ControllerManager.Controller.TryGetTransform(HumanBodyBones.Head, out var headTransform))
                originTransform.LookAt(headTransform);
            else originTransform.localPosition = Vector3.zero;

            if (!arenaPlacement.Player) return;
            for (var i = 0; i < arenaPlacement.Player.Lives.Length; i++)
                if (i >= lives.Count)
                {
                    var go = Instantiate(lifePrefab.gameObject, livesContainer);
                    var life = go.GetComponent<Slider>();
                    life.value = 1;
                    lives.Add(life);
                }
                else
                {
                    var life = lives[i];
                    lives[i].value = Mathf.Lerp(
                        life.value,
                        arenaPlacement.Player.Lives[i],
                        Time.deltaTime * 5
                    );
                }
        }

        private void party_OnGameStarted()
        {
            if (!arenaPlacement.Player || !arenaPlacement.Player.IsLocalPlayer) return;
            List<float> list = new();
            for (var i = 0; i < ArenaDescriptor.NumberOfLives; i++) list.Add(0);
            arenaPlacement.Player.Lives = list.ToArray();
        }
    }
}