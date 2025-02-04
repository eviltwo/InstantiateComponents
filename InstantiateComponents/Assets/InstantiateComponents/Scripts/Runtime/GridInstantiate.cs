using System.Collections.Generic;
using UnityEngine;

namespace InstantiateComponents
{
    [AddComponentMenu(nameof(InstantiateComponents) + "/" + nameof(GridInstantiate))]
    public class GridInstantiate : InstantiateBase
    {
        public Vector3Int Count = new Vector3Int(10, 1, 10);

        public Vector3 Spacing = new Vector3(1, 1, 1);

        protected override void OnValidate()
        {
            base.OnValidate();
            Count.x = Mathf.Max(1, Count.x);
            Count.y = Mathf.Max(1, Count.y);
            Count.z = Mathf.Max(1, Count.z);
        }

        protected override void CalculateLocalLocationsUsingRandom(List<Vector3> results)
        {
            results.Clear();

            var xCount = Mathf.Max(1, Count.x);
            var yCount = Mathf.Max(1, Count.y);
            var zCount = Mathf.Max(1, Count.z);
            for (var xIdx = 0; xIdx < xCount; xIdx++)
            {
                for (var yIdx = 0; yIdx < yCount; yIdx++)
                {
                    for (var zIdx = 0; zIdx < zCount; zIdx++)
                    {
                        var position = new Vector3(
                            xIdx * Spacing.x,
                            yIdx * Spacing.y,
                            zIdx * Spacing.z);
                        results.Add(position);
                    }
                }
            }
        }

        private static List<Location> _locationBuffer = new List<Location>();
        private static List<InstantiableItem> _itemBuffer = new List<InstantiableItem>();
        protected virtual void OnDrawGizmosSelected()
        {
            _locationBuffer.Clear();
            CalculateLocationsAndItems(_locationBuffer, _itemBuffer);
            Gizmos.color = Color.white;
            var minSpacing = Mathf.Min(Spacing.x, Spacing.y, Spacing.z);
            var size = Vector3.one * Mathf.Min(0.1f, minSpacing * 0.1f);
            foreach (var location in _locationBuffer)
            {
                Gizmos.matrix = Matrix4x4.TRS(location.position, location.rotation, location.scale);
                Gizmos.DrawWireCube(Vector3.zero, size);
            }
        }
    }
}
