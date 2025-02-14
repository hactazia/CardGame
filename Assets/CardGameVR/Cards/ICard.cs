using CardGameVR.Board;
using CardGameVR.Cards.Visual;
using Unity.Netcode;
using UnityEngine;

namespace CardGameVR.Cards
{
    public interface ICard
    {
        public Transform GetTransform();
        public VisualCard SpawnVisualCard(VisualCardHandler handler);
        public void Destroy();
    }
}