using UnityEngine;

namespace InstantiateComponents
{
    [AddComponentMenu(nameof(InstantiateComponents) + "/" + nameof(BoxInstantiate))]
    public class BoxInstantiate : ShapeInstantiate
    {
        [SerializeField]
        public Vector3 BoxSize = new Vector3(10, 0, 10);

        protected override Vector3 GetLocalBounds()
        {
            return new Vector3(BoxSize.x, BoxSize.y, BoxSize.z);
        }

        protected override float GetWeight(Vector3 localPosition)
        {
            return 1;
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
            Gizmos.color = Color.white;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(BoxSize.x, BoxSize.y, BoxSize.z));
        }
    }
}
