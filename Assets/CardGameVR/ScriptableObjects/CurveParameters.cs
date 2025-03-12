using UnityEngine;

namespace CardGameVR.ScriptableObjects
{
    [CreateAssetMenu(fileName = "CurveParameters", menuName = "CardGameVR/Hand Curve Parameters")]
    public class CurveParameters : ScriptableObject
    {
        public AnimationCurve positioning;
        public float positioningInfluence = .1f;
        public AnimationCurve rotation;
        public float rotationInfluence = 10f;
    }

}