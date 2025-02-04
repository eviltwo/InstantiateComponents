using UnityEditor;

namespace InstantiateComponents.Editor
{
    [CustomEditor(typeof(ShapeInstantiate))]
    public class ShapeInstantiateEditor : InstantiateBaseEditor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(ShapeInstantiate.Density)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(ShapeInstantiate.Spacing)));
            base.OnInspectorGUI();
        }
    }
}
