using UnityEngine;

namespace HL3.World
{
    public sealed class GrapplePoint : MonoBehaviour
    {
        [SerializeField] private Color gizmoColor = new Color(0.1f, 0.85f, 1f, 1f);
        [SerializeField] private float gizmoRadius = 0.45f;

        private void OnDrawGizmos()
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawSphere(transform.position, gizmoRadius);
            Gizmos.DrawWireSphere(transform.position, gizmoRadius * 1.8f);
        }
    }
}
