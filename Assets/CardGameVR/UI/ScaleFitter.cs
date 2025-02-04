using UnityEngine;

namespace CardGameVR.UI
{
    public class ScaleFitter : MonoBehaviour
    {
        private RectTransform _parentRect;
        private RectTransform _rect;

        private void Start()
        {
            _parentRect = transform.parent.GetComponent<RectTransform>();
            _rect = GetComponent<RectTransform>();
        }

        private void Update()
        {
            var xScale = _parentRect.rect.width / _rect.rect.width;
            var yScale = _parentRect.rect.height / _rect.rect.height;
            var scale = Mathf.Min(xScale, yScale);
            _rect.localScale = new Vector3(scale, scale, 1);
        }
    }
}