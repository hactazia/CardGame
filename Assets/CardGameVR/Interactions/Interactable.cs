using UnityEngine;

namespace CardGameVR.Interactions
{
    public abstract class Interactable : MonoBehaviour
    {
        public virtual void OnHoverEnter() {}
        public virtual void OnHoverExit() {}
        public virtual void OnSelect() {}
        public virtual void OnDeselect() {}
    }
}