using System;
using UnityEditor;
using UnityEngine;

namespace InstantiateComponents.Editor
{
    [CustomEditor(typeof(InstantiateBase), true)]
    [CanEditMultipleObjects]
    public class InstantiateBaseEditor : UnityEditor.Editor
    {
        private bool _isRandomPositionOffset;
        private static bool IsFoldoutPositionOffset;
        private bool _isRandomRotationOffset;
        private static bool IsFoldoutRotationOffset;
        private bool _isRandomScaleOffset;
        private static bool IsFoldoutScaleOffset;
        private static bool IsFoldoutTerrainSettings;

        protected virtual void OnEnable()
        {
            _isRandomPositionOffset = IsRandomRange(serializedObject.FindProperty(nameof(ShapeInstantiate.PositionOffset)));
            _isRandomRotationOffset = IsRandomRange(serializedObject.FindProperty(nameof(ShapeInstantiate.RotationOffset)));
            _isRandomScaleOffset = IsRandomRange(serializedObject.FindProperty(nameof(ShapeInstantiate.ScaleOffset)));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(ShapeInstantiate.ItemsToInstantiate)));
            DrawVector3Range(serializedObject.FindProperty(nameof(ShapeInstantiate.PositionOffset)), ref _isRandomPositionOffset, ref IsFoldoutPositionOffset);
            DrawVector3Range(serializedObject.FindProperty(nameof(ShapeInstantiate.RotationOffset)), ref _isRandomRotationOffset, ref IsFoldoutRotationOffset);
            DrawVector3Range(serializedObject.FindProperty(nameof(ShapeInstantiate.ScaleOffset)), ref _isRandomScaleOffset, ref IsFoldoutScaleOffset);
            using (var foldoutScope = new FoldoutHeaderGroupScope(IsFoldoutTerrainSettings, "Terrain"))
            using (new EditorGUI.IndentLevelScope())
            {
                IsFoldoutTerrainSettings = foldoutScope.foldout;
                if (IsFoldoutTerrainSettings)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(ShapeInstantiate.FitHeightToTerrain)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(ShapeInstantiate.FitRotationToTerrain)));
                }
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(ShapeInstantiate.RandomSeed)));

            serializedObject.ApplyModifiedProperties();
        }

        private static bool IsRandomRange(SerializedProperty vector3RangeProperty)
        {
            var min = vector3RangeProperty.FindPropertyRelative(nameof(ShapeInstantiate.Vector3Range.Min)).vector3Value;
            var max = vector3RangeProperty.FindPropertyRelative(nameof(ShapeInstantiate.Vector3Range.Max)).vector3Value;
            return min != max;
        }

        private static GUIContent BlankContent = new GUIContent(" ");
        private static void DrawVector3Range(SerializedProperty vector3RangeProperty, ref bool isRandom, ref bool foldout)
        {
            var minProperty = vector3RangeProperty.FindPropertyRelative(nameof(ShapeInstantiate.Vector3Range.Min));
            var maxProperty = vector3RangeProperty.FindPropertyRelative(nameof(ShapeInstantiate.Vector3Range.Max));

            using (new EditorGUILayout.HorizontalScope())
            using (var foldoutScope = new FoldoutHeaderGroupScope(foldout, vector3RangeProperty.displayName))
            {
                foldout = foldoutScope.foldout;
                using (var changeCheck = new EditorGUI.ChangeCheckScope())
                {
                    isRandom = GUILayout.Toggle(isRandom, "Ranmdom", GUI.skin.button, GUILayout.ExpandWidth(false));
                    if (changeCheck.changed && !isRandom)
                    {
                        maxProperty.vector3Value = minProperty.vector3Value;
                    }
                }
            }
            using (new EditorGUI.IndentLevelScope())
            {
                if (foldout)
                {
                    using (var changeCheck = new EditorGUI.ChangeCheckScope())
                    {
                        if (isRandom)
                        {
                            EditorGUILayout.PropertyField(minProperty);
                            EditorGUILayout.PropertyField(maxProperty);
                        }
                        else
                        {
                            EditorGUILayout.PropertyField(minProperty, BlankContent);
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
