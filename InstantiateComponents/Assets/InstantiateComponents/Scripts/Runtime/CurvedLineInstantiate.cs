using System.Collections.Generic;
using UnityEngine;

namespace InstantiateComponents
{
    [AddComponentMenu(nameof(InstantiateComponents) + "/" + nameof(CurvedLineInstantiate))]
    public class CurvedLineInstantiate : GridInstantiate
    {
        [Range(-45f, 45f)]
        public float AnglePerUnit = 0;

        public enum AxisKind
        {
            X = 0,
            Y = 1,
            Z = 2,
        }

        public AxisKind Axis = AxisKind.Z;

        public void Reset()
        {
            Count = new Vector3Int(1, 1, 5);
        }

        protected override void CalculateLocalLocationsUsingRandom(List<Location> results)
        {
            // Calculate grid
            base.CalculateLocalLocationsUsingRandom(results);

            if (AnglePerUnit == 0)
            {
                return;
            }

            // Modify grid to form a curved line
            for (int i = 0; i < results.Count; i++)
            {
                var p = results[i].position;
                int horizontalDim = Axis == AxisKind.X ? (int)AxisKind.Z : (int)AxisKind.X;
                int horizontalSign = Axis == AxisKind.X ? -1 : 1;
                int verticalDim = (int)Axis;
                if (p[verticalDim] == 0)
                {
                    continue;
                }

                var r = 1 / (AnglePerUnit * Mathf.Deg2Rad);
                var rad = AnglePerUnit * Mathf.Deg2Rad * p[verticalDim];
                var x = (1 - Mathf.Cos(rad)) * r * horizontalSign + Mathf.Cos(rad) * p[horizontalDim];
                var y = Mathf.Sin(rad) * r - Mathf.Sin(rad) * p[horizontalDim] * horizontalSign;
                p[horizontalDim] = x;
                p[verticalDim] = y;
                var rotationAxis = Axis == AxisKind.Y ? Vector3.back : Vector3.up;
                results[i] = new Location
                {
                    position = p,
                    rotation = Quaternion.AngleAxis(rad * Mathf.Rad2Deg, rotationAxis),
                    scale = Vector3.one,
                };
            }
        }
    }
}
