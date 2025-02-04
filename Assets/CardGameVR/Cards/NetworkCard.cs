using Unity.Netcode;
using UnityEngine;

namespace CardGameVR.Cards
{
    public class NetworkCard : NetworkBehaviour, ICard
    {
        public Transform GetTransform() => transform;
    }
}