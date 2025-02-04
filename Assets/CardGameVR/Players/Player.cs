using System;
using UnityEngine;

namespace CardGameVR.Players
{
    public abstract class Player : MonoBehaviour
    {
        public abstract void Recenter();

        public void Awake()
        {
            Recenter();
        }

        public abstract Vector3 GetPosition();
        public abstract Quaternion GetRotation();
        protected abstract void SetPosition(Vector3 position);
        protected abstract void SetRotation(Quaternion rotation);

        private void Teleport(Vector3 position, Quaternion rotation)
        {
            PlayerAnchor.Instance.transform.position = position;
            PlayerAnchor.Instance.transform.rotation = rotation;
        }

        private void Teleport(Transform t)
            => Teleport(t.position, t.rotation);

        public bool TryCast<T>(out T player) where T : Player
        {
            player = this as T;
            return player;
        }
    }
}