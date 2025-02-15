using UnityEditor;

namespace InstantiateComponents.Editor
{
    [CustomEditor(typeof(GridInstantiate), true)]
    [CanEditMultipleObjects]
    public class GridInstantiateEditor : InstantiateBaseEditor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(GridInstantiate.Count)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(GridInstantiate.Spacing)));
            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI();
        }
    }
}
