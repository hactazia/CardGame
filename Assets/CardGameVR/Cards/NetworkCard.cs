using CardGameVR.Cards.Visual;
using Unity.Netcode;
using UnityEngine;

namespace CardGameVR.Cards
{
    public class NetworkCard : NetworkBehaviour, ICard
    {
        public Transform GetTransform() => transform;

        public VisualCard SpawnVisualCard(VisualCardHandler handler)
        {
            throw new System.NotImplementedException();
        }

        public void Destroy()
        {
            throw new System.NotImplementedException();
        }
    }
}