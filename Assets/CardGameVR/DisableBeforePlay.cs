#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace CardGameVR
{
#if UNITY_EDITOR
    [ExecuteInEditMode]
#endif
    public class DisableBeforePlay : MonoBehaviour
    {
#if UNITY_EDITOR
        private void OnEnable()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                gameObject.SetActive(false);
                Debug.Log($"{gameObject.name} désactivé avant le Play Mode.");
            }
        }
#endif
    }
}