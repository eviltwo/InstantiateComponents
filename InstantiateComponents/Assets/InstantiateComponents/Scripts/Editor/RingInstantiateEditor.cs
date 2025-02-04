using UnityEditor;

namespace InstantiateComponents.Editor
{
    [CustomEditor(typeof(RingInstantiate))]
    public class RingInstantiateEditor : InstantiateBaseEditor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(RingInstantiate.Count)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(RingInstantiate.Radius)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(RingInstantiate.Angle)));

            serializedObject.ApplyModifiedProperties();


            base.OnInspectorGUI();
        }
    }
}
