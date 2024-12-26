using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace InstantiateComponents
{
    [ExecuteInEditMode]
    [SelectionBase]
    public abstract class ShapeInstantiate : MonoBehaviour
    {
        [Serializable]
        public class InstantiableItem
        {
            public GameObject Prefab = null;
            public float Probability = 1f;
        }

        [SerializeField]
        public List<InstantiableItem> ItemsToInstantiate = new List<InstantiableItem>();

        [SerializeField]
        public float Density = 0.5f;

        [SerializeField]
        public float Spacing = 0.5f;

        [Serializable]
        public class Vector3Range
        {
            public Vector3 Min = Vector3.zero;
            public Vector3 Max = Vector3.zero;
        }

        [SerializeField]
        public Vector3Range PositionOffset = default;

        [SerializeField]
        public Vector3Range RotationOffset = default;

        [SerializeField]
        public Vector3Range ScaleOffset = default;

        [SerializeField, Range(0, 1)]
        public float FitHeightToTerrain = 0f;

        [SerializeField, Range(0, 1)]
        public float FitRotationToTerrain = 0f;

        [SerializeField]
        public int RandomSeed = 0;

        public int CountLimit { get; set; } = 10000;
        private bool _isDirty;

        private struct Location
        {
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 scale;
        }

        private void OnEnable()
        {
            if (RandomSeed == 0)
            {
                RandomSeed = GetInstanceID();
            }
            SetDirty();
        }

        private void OnDisable()
        {
            ClearInstances();
        }

        private void OnValidate()
        {
            SetDirty();
        }

        public void SetDirty()
        {
            _isDirty = true;
        }

        private void Update()
        {
            if (_isDirty && isActiveAndEnabled)
            {
                _isDirty = false;
                UpdateInstances();
            }
        }

        private static List<Location> _locationBuffer = new List<Location>();
        private void UpdateInstances()
        {
            ClearInstances();

            if (ItemsToInstantiate.Count == 0)
            {
                return;
            }

            var randomStateCache = Random.state;
            Random.InitState(RandomSeed);
            try
            {
                // Setup random box
                var randomBox = new RandomBox();
                foreach (var item in ItemsToInstantiate)
                {
                    randomBox.PushContent(item.Probability);
                }

                // Calculate locations
                _locationBuffer.Clear();
                CalculateLocations(_locationBuffer);

                // Instantiate
                foreach (var location in _locationBuffer)
                {
                    var item = ItemsToInstantiate[randomBox.Choose()];
                    if (item.Prefab != null)
                    {
                        var instance = CreateInstance(item.Prefab);
                        instance.transform.position = location.position;
                        instance.transform.rotation = location.rotation;
                        instance.transform.localScale = location.scale;
                    }
                }
            }
            finally
            {
                Random.state = randomStateCache;
            }
        }

        protected abstract Vector3 GetLocalBounds();

        protected abstract float GetWeight(Vector3 localPosition);

        private void CalculateLocations(List<Location> results)
        {
            results.Clear();

            var bounds = GetLocalBounds();
            var cellSize = 1 / Mathf.Max(0.001f, Density);
            var cellInnerSize = Mathf.Max(0, cellSize - Spacing);
            var xCount = Mathf.Max(1, Mathf.FloorToInt(bounds.x / cellSize));
            var yCount = Mathf.Max(1, Mathf.FloorToInt(bounds.y / cellSize));
            var zCount = Mathf.Max(1, Mathf.FloorToInt(bounds.z / cellSize));
            var xStart = -cellSize * (xCount - 1) * 0.5f;
            var yStart = -cellSize * (yCount - 1) * 0.5f;
            var zStart = -cellSize * (zCount - 1) * 0.5f;
            for (var xIdx = 0; xIdx < xCount; xIdx++)
            {
                for (var yIdx = 0; yIdx < yCount; yIdx++)
                {
                    for (var zIdx = 0; zIdx < zCount; zIdx++)
                    {
                        var cellCenter = new Vector3(
                            xStart + xIdx * cellSize,
                            yStart + yIdx * cellSize,
                            zStart + zIdx * cellSize);
                        var offset = new Vector3(
                            cellInnerSize * Random.Range(-0.5f, 0.5f) * Mathf.Clamp01(bounds.x / cellSize),
                            cellInnerSize * Random.Range(-0.5f, 0.5f) * Mathf.Clamp01(bounds.y / cellSize),
                            cellInnerSize * Random.Range(-0.5f, 0.5f) * Mathf.Clamp01(bounds.z / cellSize));
                        var localPivot = cellCenter + offset;
                        var weight = GetWeight(localPivot);
                        if (weight <= 0)
                        {
                            continue;
                        }
                        var worldPosition = transform.position + transform.rotation * localPivot;
                        CalculateTRS(worldPosition, out var pos, out var rot, out var scale);
                        results.Add(new Location
                        {
                            position = pos,
                            rotation = rot,
                            scale = scale,
                        });
                    }
                }
            }

            if (results.Count > CountLimit)
            {
                results.RemoveRange(CountLimit, results.Count - CountLimit);
            }
        }

        protected virtual void CalculateTRS(
            Vector3 worldPosition,
            out Vector3 position,
            out Quaternion rotation,
            out Vector3 scale)
        {
            // Position
            position = worldPosition;
            var posOffset = Vector3.Lerp(PositionOffset.Min, PositionOffset.Max, Random.value);
            position += transform.rotation * posOffset;
            if (FitHeightToTerrain > 0)
            {
                var terrain = Terrain.activeTerrain;
                if (terrain != null)
                {
                    var terrainHeight = terrain.SampleHeight(position) + terrain.GetPosition().y;
                    terrainHeight += posOffset.y;
                    position.y = Mathf.Lerp(position.y, terrainHeight, FitHeightToTerrain);
                }
            }

            // Rotation
            var rotOffset = Vector3.Lerp(RotationOffset.Min, RotationOffset.Max, Random.value);
            rotation = Quaternion.Euler(rotOffset) * transform.rotation;
            if (FitRotationToTerrain > 0)
            {
                var terrain = Terrain.activeTerrain;
                if (terrain != null)
                {
                    var localPosition = terrain.transform.InverseTransformPoint(position);
                    var normal = terrain.terrainData.GetInterpolatedNormal(localPosition.x / terrain.terrainData.size.x, localPosition.z / terrain.terrainData.size.z);
                    var terrainRotation = Quaternion.Lerp(Quaternion.identity, Quaternion.FromToRotation(Vector3.up, normal), FitRotationToTerrain);
                    rotation = terrainRotation * rotation;
                }
            }

            // Scale
            var scaleOffset = Vector3.Lerp(ScaleOffset.Min, ScaleOffset.Max, Random.value);
            scale = Vector3.one + scaleOffset;
        }

        private GameObject _instanceRoot;
        public GameObject InstanceRoot => _instanceRoot;

        private static readonly List<ShapeInstantiate> _componentBuffer = new List<ShapeInstantiate>();
        private static readonly List<GameObject> _unusedObjectBuffer = new List<GameObject>();

        private void ClearInstances()
        {
            if (Application.isPlaying)
            {
                Destroy(_instanceRoot);
            }
            else
            {
                DestroyImmediate(_instanceRoot);
            }
            _instanceRoot = null;

            // Destroy unused root objects.
            _componentBuffer.Clear();
            GetComponents(_componentBuffer);
            _unusedObjectBuffer.Clear();
            var childCount = transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                var child = transform.GetChild(i);
                if ((child.gameObject.hideFlags & HideFlags.DontSaveInEditor) == 0)
                {
                    continue;
                }
                var used = false;
                foreach (var component in _componentBuffer)
                {
                    if (component.InstanceRoot == child.gameObject)
                    {
                        used = true;
                        break;
                    }
                }
                if (!used)
                {
                    _unusedObjectBuffer.Add(child.gameObject);
                }
            }
            foreach (var destroyObject in _unusedObjectBuffer)
            {
                if (Application.isPlaying)
                {
                    Destroy(destroyObject);
                }
                else
                {
                    DestroyImmediate(destroyObject);
                }
            }
        }

        private GameObject CreateInstance(GameObject prefab)
        {
            if (_instanceRoot == null)
            {
                _instanceRoot = new GameObject("SplineSpanInstanceRoot");
                _instanceRoot.hideFlags |= HideFlags.HideAndDontSave;
                //InstanceRoot.hideFlags |= HideFlags.NotEditable | HideFlags.DontSave;
                _instanceRoot.transform.SetParent(transform, false);
            }
            var instance = Instantiate(prefab, _instanceRoot.transform);
            instance.hideFlags |= HideFlags.DontSave;
            instance.transform.SetParent(_instanceRoot.transform, false);
            return instance;
        }

        protected virtual void OnDrawGizmosSelected()
        {
            var randomStateCache = Random.state;
            Random.InitState(RandomSeed);
            try
            {
                _locationBuffer.Clear();
                CalculateLocations(_locationBuffer);
                Gizmos.color = Color.white;
                foreach (var location in _locationBuffer)
                {
                    Gizmos.matrix = Matrix4x4.TRS(location.position, location.rotation, location.scale);
                    Gizmos.DrawWireSphere(Vector3.zero, Spacing);
                }
            }
            finally
            {
                Random.state = randomStateCache;
            }
        }
    }

    internal class RandomBox
    {
        private List<float> _weights = new List<float>();
        private float _totalWeight = 0f;

        public void ClearContents()
        {
            _weights.Clear();
            _totalWeight = 0f;
        }

        public void PushContent(float weight)
        {
            _weights.Add(weight);
            _totalWeight += weight;
        }

        public int Choose()
        {
            var randomValue = UnityEngine.Random.Range(0f, _totalWeight);
            for (int i = 0; i < _weights.Count; i++)
            {
                randomValue -= _weights[i];
                if (randomValue <= 0f)
                {
                    return i;
                }
            }
            return _weights.Count - 1;
        }
    }
}
