using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace CardGameVR.Interactions
{
    public class EventInteractable : Interactable
    {
        [HideInInspector] public UnityEvent onSelectEvent = new();
        [HideInInspector] public UnityEvent onDeselectEvent = new();
        [HideInInspector] public UnityEvent onHoverEnterEvent = new();
        [HideInInspector] public UnityEvent onHoverExitEvent = new();

        public override void OnSelect()
            => onSelectEvent.Invoke();

        public override void OnDeselect()
            => onDeselectEvent.Invoke();

        public override void OnHoverEnter()
            => onHoverEnterEvent.Invoke();

        public override void OnHoverExit()
            => onHoverExitEvent.Invoke();
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(EventInteractable))]
    public class EventInteractableEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            UnityEditor.EditorGUILayout.Space();

            UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("onSelectEvent"));
            UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("onDeselectEvent"));
            UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("onHoverEnterEvent"));
            UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("onHoverExitEvent"));

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}