using UnityEditor;

namespace InstantiateComponents.Editor
{
    [CustomEditor(typeof(BoxInstantiate))]
    public class BoxInstantiateEditor : ShapeInstantiateEditor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(BoxInstantiate.BoxSize)));

            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI();
        }
    }
}