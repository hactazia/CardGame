using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class DisableBeforePlay : MonoBehaviour
{
    private void OnEnable()
    {
        // Vérifie si l'éditeur est en mode édition (pas en Play Mode)
        if (!EditorApplication.isPlayingOrWillChangePlaymode)
        {
            gameObject.SetActive(false);
            Debug.Log($"{gameObject.name} désactivé avant le Play Mode.");
        }
    }
}