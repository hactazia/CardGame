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

    }
}