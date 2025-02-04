using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace InstantiateComponents
{
    [ExecuteInEditMode]
    [SelectionBase]
    public abstract class InstantiateBase : MonoBehaviour
    {
        [Serializable]
        public class InstantiableItem
        {
            public GameObject Prefab = null;
            public float Probability = 1f;
        }

        [SerializeField]
        public List<InstantiableItem> ItemsToInstantiate = new List<InstantiableItem>()
        {
            new InstantiableItem { Prefab = null, Probability = 1f },
        };

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

        private bool _isDirty;

        private Vector3 _lastPosition;
        private Quaternion _lastRotation;

        public int CountLimit { get; set; } = 10000;

        protected struct Location
        {
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 scale;
        }

        protected virtual void OnEnable()
        {
            if (RandomSeed == 0)
            {
                RandomSeed = GetInstanceID();
            }
            _lastPosition = transform.position;
            _lastRotation = transform.rotation;
            SetDirty();
        }

        protected virtual void OnDisable()
        {
            ClearControlledInstance();
            ClearUnusedInstances();
        }

        protected virtual void OnValidate()
        {
            if (!Application.isPlaying)
            {
                SetDirty();
            }
        }

        public void SetDirty()
        {
            _isDirty = true;
        }

        protected virtual void Update()
        {
            if (!Application.isPlaying && (_lastPosition != transform.position || _lastRotation != transform.rotation))
            {
                _lastPosition = transform.position;
                _lastRotation = transform.rotation;
                SetDirty();
            }

            if (_isDirty && isActiveAndEnabled)
            {
                _isDirty = false;
                UpdateInstances();
            }
        }

        private static List<int> _usedPrefabIds = new List<int>();
        private static List<Location> _locationBuffer = new List<Location>();
        private static List<InstantiableItem> _itemBuffer = new List<InstantiableItem>();
        private void UpdateInstances()
        {
            if (ItemsToInstantiate.Count == 0)
            {
                return;
            }

            _locationBuffer.Clear();
            _itemBuffer.Clear();
            CalculateLocationsAndItems(_locationBuffer, _itemBuffer);

            // Check rebuild
            var requireRebuild = false;
            requireRebuild |= _locationBuffer.Count != _usedPrefabIds.Count;

            // Check prefab id
            if (!requireRebuild)
            {
                var diffId = false;
                for (var i = 0; i < _itemBuffer.Count; i++)
                {
                    var instanceId = _itemBuffer[i].Prefab?.GetInstanceID() ?? 0;
                    if (instanceId != _usedPrefabIds[i])
                    {
                        diffId = true;
                        break;
                    }
                }
                requireRebuild |= diffId;
            }

            // Check instance count
            requireRebuild |= _instanceRoot == null;
            if (!requireRebuild)
            {
                var childCount = _instanceRoot.transform.childCount;
                var validSourceCount = 0;
                foreach (var sourceId in _usedPrefabIds)
                {
                    if (sourceId != 0)
                    {
                        validSourceCount++;
                    }
                }
                requireRebuild |= childCount != validSourceCount;
            }

            // Rebuild instances
            if (requireRebuild)
            {
                _usedPrefabIds.Clear();
                ClearControlledInstance();
                foreach (var item in _itemBuffer)
                {
                    if (item.Prefab == null)
                    {
                        _usedPrefabIds.Add(0);
                    }
                    else
                    {
                        var instance = CreateInstance(item.Prefab);
                        _usedPrefabIds.Add(item.Prefab.GetInstanceID());
                    }
                }
            }

            // Set locations
            {
                var childCount = _instanceRoot == null ? 0 : _instanceRoot.transform.childCount;
                var childIndex = 0;
                for (var i = 0; i < _locationBuffer.Count; i++)
                {
                    if (_itemBuffer[i] == null || _itemBuffer[i].Prefab == null)
                    {
                        continue;
                    }
                    if (childIndex >= childCount)
                    {
                        Debug.LogError($"Child count mismatch: {childIndex} >= {childCount}");
                        break;
                    }
                    var instance = _instanceRoot.transform.GetChild(childIndex++).gameObject;
                    var location = _locationBuffer[i];
                    instance.transform.position = location.position;
                    instance.transform.rotation = location.rotation;
                    instance.transform.localScale = location.scale;
                }
                if (childIndex < childCount)
                {
                    Debug.LogError($"Child count mismatch: {childIndex} != {childCount}");
                }
            }

            // Clear lists
            _locationBuffer.Clear();
            _itemBuffer.Clear();

            ClearUnusedInstances();
        }

        private static List<Vector3> _localPositionBuffer = new List<Vector3>();
        protected void CalculateLocationsAndItems(List<Location> resultLocations, List<InstantiableItem> resultItems)
        {
            resultLocations.Clear();
            resultItems.Clear();

            // Setup random box
            var randomBox = new RandomBox();
            foreach (var item in ItemsToInstantiate)
            {
                randomBox.PushContent(item.Probability);
            }

            var randomStateCache = Random.state;
            Random.InitState(RandomSeed);
            try
            {
                // Get local positions
                _localPositionBuffer.Clear();
                CalculateLocalLocationsUsingRandom(_localPositionBuffer);
                if (_localPositionBuffer.Count > CountLimit)
                {
                    _localPositionBuffer.RemoveRange(CountLimit, _localPositionBuffer.Count - CountLimit);
                }

                // Translate to world positions
                for (var i = 0; i < _localPositionBuffer.Count; i++)
                {
                    var location = new Location();
                    GetWorldTRS(_localPositionBuffer[i], out location.position, out location.rotation, out location.scale);
                    resultLocations.Add(location);
                }

                // Choose items
                for (var i = 0; i < _localPositionBuffer.Count; i++)
                {
                    resultItems.Add(ItemsToInstantiate[randomBox.Choose()]);
                }
            }
            finally
            {
                Random.state = randomStateCache;
            }
        }

        protected abstract void CalculateLocalLocationsUsingRandom(List<Vector3> results);

        private void GetWorldTRS(
            Vector3 localPosition,
            out Vector3 position,
            out Quaternion rotation,
            out Vector3 scale)
        {
            var worldPosition = transform.position + transform.rotation * localPosition;

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
                    var terrainLocal = terrain.transform.InverseTransformPoint(position);
                    var normal = terrain.terrainData.GetInterpolatedNormal(terrainLocal.x / terrain.terrainData.size.x, terrainLocal.z / terrain.terrainData.size.z);
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

        private void ClearControlledInstance()
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
        }

        private static readonly List<InstantiateBase> _componentBuffer = new List<InstantiateBase>();
        private static readonly List<GameObject> _unusedObjectBuffer = new List<GameObject>();
        private void ClearUnusedInstances()
        {
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

            // Clear lists
            _componentBuffer.Clear();
            _unusedObjectBuffer.Clear();
        }

        private GameObject CreateInstance(GameObject prefab)
        {
            if (_instanceRoot == null)
            {
                _instanceRoot = new GameObject("InstanceRoot");
                _instanceRoot.hideFlags |= HideFlags.HideAndDontSave;
                _instanceRoot.transform.SetParent(transform, false);
            }
            var instance = Instantiate(prefab, _instanceRoot.transform);
            instance.hideFlags |= HideFlags.DontSave;
            instance.transform.SetParent(_instanceRoot.transform, false);
            return instance;
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
