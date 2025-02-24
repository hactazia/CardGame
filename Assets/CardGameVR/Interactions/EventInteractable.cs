using UnityEngine.Events;
using UnityEngine.Serialization;

namespace CardGameVR.Interactions
{
    public class EventInteractable : Interactable
    {
        public UnityEvent onSelectEvent = new();
        public UnityEvent onDeselectEvent = new();
        public UnityEvent onHoverEnterEvent = new();
        public UnityEvent onHoverExitEvent = new();

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
        private EventInteractable _eventInteractable;

        private void OnEnable()
        {
            _eventInteractable = (EventInteractable)target;
        }

        public override void OnInspectorGUI()
        {
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