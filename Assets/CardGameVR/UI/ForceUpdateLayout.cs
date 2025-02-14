using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace CardGameVR.UI
{
    public interface UpdateLayout
    {
        void UpdateLayout()
        {
        }
    }

    public class ForceUpdateLayout
    {
        public static void UpdateManually(GameObject go) => UpdateManually(go.GetComponent<RectTransform>());

        public static void UpdateManually(RectTransform rect)
        {
            if (!rect || !rect.gameObject.activeInHierarchy) return;

            var layoutElement = rect.GetComponent<LayoutElement>();
            if (layoutElement && layoutElement.ignoreLayout) return;

            foreach (Transform child in rect)
                if (child.TryGetComponent<RectTransform>(out var rec))
                    UpdateManually(rec);
            
            var rectTransform = rect.GetComponent<RectTransform>();
            var contentSizeFitter = rect.GetComponent<ContentSizeFitter>();
            var layoutGroup = rect.GetComponent<LayoutGroup>();

            if (contentSizeFitter)
            {
                contentSizeFitter.SetLayoutHorizontal();
                contentSizeFitter.SetLayoutVertical();
            }

            if (layoutGroup)
            {
                layoutGroup.CalculateLayoutInputHorizontal();
                layoutGroup.CalculateLayoutInputVertical();
                layoutGroup.SetLayoutHorizontal();
                layoutGroup.SetLayoutVertical();
            }

            foreach (var child in rect.GetComponents<UpdateLayout>())
                child.UpdateLayout();

            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        }
    }
}