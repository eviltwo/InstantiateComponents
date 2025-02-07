using UnityEditor;

namespace InstantiateComponents.Editor
{
    [CustomEditor(typeof(CurvedLineInstantiate), true)]
    [CanEditMultipleObjects]
    public class CurvedLineInstantiateEditor : GridInstantiateEditor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(CurvedLineInstantiate.AnglePerUnit)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(CurvedLineInstantiate.Axis)));

            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI();
        }
    }
}
