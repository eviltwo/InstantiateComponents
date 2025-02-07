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
        private bool _isRandomUniformScaleOffset;
        private static bool IsFoldoutUniformScaleOffset;
        private static bool IsFoldoutTerrainSettings;

        protected virtual void OnEnable()
        {
            _isRandomPositionOffset = IsRandomRangeVector3(serializedObject.FindProperty(nameof(InstantiateBase.PositionOffset)));
            _isRandomRotationOffset = IsRandomRangeVector3(serializedObject.FindProperty(nameof(InstantiateBase.RotationOffset)));
            _isRandomScaleOffset = IsRandomRangeVector3(serializedObject.FindProperty(nameof(InstantiateBase.ScaleOffset)));
            _isRandomUniformScaleOffset = IsRandomRangeFloat(serializedObject.FindProperty(nameof(InstantiateBase.UniformScaleOffset)));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(InstantiateBase.ItemsToInstantiate)));
            DrawVector3Range(serializedObject.FindProperty(nameof(InstantiateBase.PositionOffset)), ref _isRandomPositionOffset, ref IsFoldoutPositionOffset);
            DrawVector3Range(serializedObject.FindProperty(nameof(InstantiateBase.RotationOffset)), ref _isRandomRotationOffset, ref IsFoldoutRotationOffset);
            DrawVector3Range(serializedObject.FindProperty(nameof(InstantiateBase.ScaleOffset)), ref _isRandomScaleOffset, ref IsFoldoutScaleOffset);
            DrwaFloatRange(serializedObject.FindProperty(nameof(InstantiateBase.UniformScaleOffset)), ref _isRandomUniformScaleOffset, ref IsFoldoutUniformScaleOffset);
            using (var foldoutScope = new FoldoutHeaderGroupScope(IsFoldoutTerrainSettings, "Terrain"))
            using (new EditorGUI.IndentLevelScope())
            {
                IsFoldoutTerrainSettings = foldoutScope.foldout;
                if (IsFoldoutTerrainSettings)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(InstantiateBase.FitHeightToTerrain)));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(InstantiateBase.FitRotationToTerrain)));
                }
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(InstantiateBase.RandomSeed)));

            serializedObject.ApplyModifiedProperties();
        }

        private static bool IsRandomRangeFloat(SerializedProperty floatRangeProperty)
        {
            var min = floatRangeProperty.FindPropertyRelative(nameof(InstantiateBase.FloatRange.Min)).floatValue;
            var max = floatRangeProperty.FindPropertyRelative(nameof(InstantiateBase.FloatRange.Max)).floatValue;
            return min != max;
        }

        private static bool IsRandomRangeVector3(SerializedProperty vector3RangeProperty)
        {
            var min = vector3RangeProperty.FindPropertyRelative(nameof(InstantiateBase.Vector3Range.Min)).vector3Value;
            var max = vector3RangeProperty.FindPropertyRelative(nameof(InstantiateBase.Vector3Range.Max)).vector3Value;
            return min != max;
        }

        private static GUIContent BlankContent = new GUIContent(" ");
        private static void DrawVector3Range(SerializedProperty vector3RangeProperty, ref bool isRandom, ref bool foldout)
        {
            var minProperty = vector3RangeProperty.FindPropertyRelative(nameof(InstantiateBase.Vector3Range.Min));
            var maxProperty = vector3RangeProperty.FindPropertyRelative(nameof(InstantiateBase.Vector3Range.Max));

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

        private static void DrwaFloatRange(SerializedProperty floatRangeProperty, ref bool isRandom, ref bool foldout)
        {
            var minProperty = floatRangeProperty.FindPropertyRelative(nameof(InstantiateBase.FloatRange.Min));
            var maxProperty = floatRangeProperty.FindPropertyRelative(nameof(InstantiateBase.FloatRange.Max));

            using (new EditorGUILayout.HorizontalScope())
            using (var foldoutScope = new FoldoutHeaderGroupScope(foldout, floatRangeProperty.displayName))
            {
                foldout = foldoutScope.foldout;
                using (var changeCheck = new EditorGUI.ChangeCheckScope())
                {
                    isRandom = GUILayout.Toggle(isRandom, "Ranmdom", GUI.skin.button, GUILayout.ExpandWidth(false));
                    if (changeCheck.changed && !isRandom)
                    {
                        maxProperty.floatValue = minProperty.floatValue;
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
                            maxProperty.floatValue = minProperty.floatValue;
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
