using System;
using UnityEditor;
using UnityEngine;

namespace InstantiateComponents.Editor
{
    [CustomEditor(typeof(ShapeInstantiate))]
    public class ShapeInstantiateEditor : UnityEditor.Editor
    {
        private bool _isRandomPositionOffset;
        private static bool IsFoldoutPositionOffset;

        protected virtual void OnEnable()
        {
            _isRandomPositionOffset = IsRandomRange(serializedObject.FindProperty(nameof(ShapeInstantiate.PositionOffset)));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(ShapeInstantiate.ItemsToInstantiate)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(ShapeInstantiate.Density)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(ShapeInstantiate.Spacing)));
            DrawVector3Range(serializedObject.FindProperty(nameof(ShapeInstantiate.PositionOffset)), ref _isRandomPositionOffset, ref IsFoldoutPositionOffset);

            serializedObject.ApplyModifiedProperties();
        }

        private static bool IsRandomRange(SerializedProperty vector3RangeProperty)
        {
            var min = vector3RangeProperty.FindPropertyRelative(nameof(ShapeInstantiate.Vector3Range.Min)).vector3Value;
            var max = vector3RangeProperty.FindPropertyRelative(nameof(ShapeInstantiate.Vector3Range.Max)).vector3Value;
            return min != max;
        }

        private static void DrawVector3Range(SerializedProperty vector3RangeProperty, ref bool isRandom, ref bool foldout)
        {
            var minProperty = vector3RangeProperty.FindPropertyRelative(nameof(ShapeInstantiate.Vector3Range.Min));
            var maxProperty = vector3RangeProperty.FindPropertyRelative(nameof(ShapeInstantiate.Vector3Range.Max));

            using (var foldoutScope = new FoldoutHeaderGroupScope(foldout, vector3RangeProperty.displayName))
            using (new EditorGUI.IndentLevelScope())
            {
                foldout = foldoutScope.foldout;
                if (foldout)
                {
                    using (var changeCheck = new EditorGUI.ChangeCheckScope())
                    {
                        isRandom = EditorGUILayout.Toggle("Random", isRandom);
                        if (changeCheck.changed && !isRandom)
                        {
                            maxProperty.vector3Value = minProperty.vector3Value;
                        }
                    }

                    using (var changeCheck = new EditorGUI.ChangeCheckScope())
                    {
                        if (isRandom)
                        {
                            EditorGUILayout.PropertyField(minProperty);
                            EditorGUILayout.PropertyField(maxProperty);
                        }
                        else
                        {
                            EditorGUILayout.PropertyField(minProperty, GUIContent.none);
                        }

                        if (changeCheck.changed && !isRandom)
                        {
                            maxProperty.vector3Value = minProperty.vector3Value;
                        }
                    }
                }
            }


        }
    }

    public class FoldoutHeaderGroupScope : IDisposable
    {
        public bool foldout;

        public FoldoutHeaderGroupScope(bool foldout, string label)
        {
            this.foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, label);
        }

        public void Dispose()
        {
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}
