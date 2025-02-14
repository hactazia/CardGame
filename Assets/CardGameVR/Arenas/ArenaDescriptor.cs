using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace CardGameVR.Arenas
{
    /**
     * ArenaDescriptor is a singleton class that holds the current arena's information.
     * one arena by time and unique by scene.
     * It's make information for placing players and game board in the scene.
     */
    public class ArenaDescriptor : MonoBehaviour
    {
        public static ArenaDescriptor Instance;

        public ArenaPlacement[] placements = Array.Empty<ArenaPlacement>();

        public void Awake()
        {
            Instance = this;
        }
    }
}