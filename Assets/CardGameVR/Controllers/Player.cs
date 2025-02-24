using CardGameVR.UI;
using UnityEngine;

namespace CardGameVR.Controllers
{
    public abstract class Controller : MonoBehaviour
    {
        public abstract void Recenter();
        public Menu menu;

        public virtual void Awake() => Recenter();

        public abstract Vector3 GetPosition();
        public abstract Quaternion GetRotation();
        protected abstract void SetPosition(Vector3 position);
        protected abstract void SetRotation(Quaternion rotation);

        public void Teleport(Vector3 position, Quaternion rotation)
        {
            SetPosition(position);
            SetRotation(rotation);
        }

        public void Teleport(Transform t)
            => Teleport(t.position, t.rotation);

        public virtual bool TryGetTransform(HumanBodyBones bone, out Transform t)
        {
            var animator = GetComponent<Animator>();
            if (animator)
            {
                t = animator.GetBoneTransform(bone);
                return t;
            }

            t = null;
            return false;
        }

        public bool TryCast<T>(out T player) where T : Controller
        {
            player = this as T;
            return player;
        }
    }
}