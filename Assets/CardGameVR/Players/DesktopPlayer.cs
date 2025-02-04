using UnityEngine;

namespace CardGameVR.Players
{
    public class DesktopPlayer : Player
    {
        public Camera playerCamera;

        public override void Recenter() { }
        
        public override Vector3 GetPosition()
            => transform.position;
        
        public override Quaternion GetRotation()
            => playerCamera.transform.rotation;
        
        protected override void SetPosition(Vector3 position)
            => transform.position = position;
        
        protected override void SetRotation(Quaternion rotation)
        {
            transform.rotation = new Quaternion(
                0, rotation.y,
                0, rotation.w
            );
            playerCamera.transform.rotation = rotation;
        }
    }
}