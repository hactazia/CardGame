using CardGameVR.Board;
using Unity.Netcode;
using UnityEngine;

namespace CardGameVR.Cards
{
    public interface ICard
    {
        public Transform GetTransform();
    }
}