using System.Collections.Generic;
using UnityEngine;

namespace InstantiateComponents
{
    [AddComponentMenu(nameof(InstantiateComponents) + "/" + nameof(RingInstantiate))]
    public class RingInstantiate : InstantiateBase
    {
        public float Count = 8;

        public float Radius = 1;

        public float Angle = 0;

        protected override void CalculateLocalLocationsUsingRandom(List<Location> results)
        {
            for (int i = 0; i < Count; i++)
            {
                var angle = Angle * Mathf.Deg2Rad + i * Mathf.PI * 2 / Count;
                var x = Mathf.Sin(angle) * Radius;
                var z = Mathf.Cos(angle) * Radius;
                results.Add(new Location
                {
                    position = new Vector3(x, 0, z),
                    rotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.up),
                    scale = Vector3.one,
                });
            }
        }
    }
}
