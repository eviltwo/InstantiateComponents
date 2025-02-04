using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace InstantiateComponents
{
    [ExecuteInEditMode]
    [SelectionBase]
    public abstract class ShapeInstantiate : InstantiateBase
    {
        [SerializeField]
        public float Density = 0.5f;

        [SerializeField]
        public float Spacing = 0.5f;

        protected override void OnValidate()
        {
            base.OnValidate();
            Density = Mathf.Max(0, Density);
            Spacing = Mathf.Max(0, Spacing);
        }

        protected abstract Vector3 GetLocalBounds();

        protected abstract float GetWeight(Vector3 localPosition);

        protected override void CalculateLocalLocationsUsingRandom(List<Vector3> results)
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
                        var position = cellCenter + offset;
                        var weight = GetWeight(position);
                        if (weight <= 0)
                        {
                            continue;
                        }

                        results.Add(position);
                    }
                }
            }

            if (results.Count > CountLimit)
            {
                results.RemoveRange(CountLimit, results.Count - CountLimit);
            }
        }

        private static List<Location> _locationBuffer = new List<Location>();
        private static List<InstantiableItem> _itemBuffer = new List<InstantiableItem>();
        protected virtual void OnDrawGizmosSelected()
        {
            _locationBuffer.Clear();
            CalculateLocationsAndItems(_locationBuffer, _itemBuffer);
            Gizmos.color = Color.white;
            foreach (var location in _locationBuffer)
            {
                Gizmos.matrix = Matrix4x4.TRS(location.position, location.rotation, location.scale);
                Gizmos.DrawWireSphere(Vector3.zero, Spacing);
            }
        }
    }
}
