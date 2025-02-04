using UnityEditor;

namespace InstantiateComponents.Editor
{
    [CustomEditor(typeof(ShapeInstantiate))]
    public class ShapeInstantiateEditor : InstantiateBaseEditor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(ShapeInstantiate.Density)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(ShapeInstantiate.Spacing)));
            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI();
        }
    }
}
