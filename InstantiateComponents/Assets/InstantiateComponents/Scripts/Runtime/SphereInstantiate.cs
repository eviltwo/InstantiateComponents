using UnityEngine;

namespace InstantiateComponents
{
    [AddComponentMenu(nameof(InstantiateComponents) + "/" + nameof(SphereInstantiate))]
    public class SphereInstantiate : ShapeInstantiate
    {
        [SerializeField]
        public Vector3 SphereSize = new Vector3(10, 0, 10);

        protected override Vector3 GetLocalBounds()
        {
            return new Vector3(SphereSize.x, SphereSize.y, SphereSize.z);
        }

        protected override float GetWeight(Vector3 localPosition)
        {
            var spherePos = new Vector3(localPosition.x / SphereSize.x, localPosition.y / SphereSize.y, localPosition.z / SphereSize.z);
            for (int i = 0; i < 3; i++)
            {
                if (SphereSize[i] == 0)
                {
                    spherePos[i] = 0;
                }
            }
            return spherePos.sqrMagnitude < 0.5f * 0.5f ? 1 : 0;
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
            Gizmos.color = Color.white;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, SphereSize);
            Gizmos.DrawWireSphere(Vector3.zero, 0.5f);
        }
    }
}
