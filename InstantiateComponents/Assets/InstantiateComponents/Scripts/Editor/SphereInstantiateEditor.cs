using UnityEditor;

namespace InstantiateComponents.Editor
{
    [CustomEditor(typeof(SphereInstantiate))]
    public class SphereInstantiateEditor : ShapeInstantiateEditor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SphereInstantiate.SphereSize)));

            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI();
        }
    }
}
