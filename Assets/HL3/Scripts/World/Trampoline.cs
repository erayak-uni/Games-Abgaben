using UnityEngine;
using HL3.Player;

namespace HL3.World
{
    public sealed class Trampoline : MonoBehaviour
    {
        [SerializeField] private float upwardVelocity = 22f;
        [SerializeField] private float outwardVelocity = 2f;
        [SerializeField] private Transform directionReference;

        public Vector3 GetLaunchVelocity(Vector3 actorPosition)
        {
            Vector3 outward = directionReference != null
                ? directionReference.forward
                : (actorPosition - transform.position).normalized;

            outward.y = 0f;
            if (outward.sqrMagnitude < 0.001f)
            {
                outward = transform.forward;
            }

            return outward.normalized * outwardVelocity + Vector3.up * upwardVelocity;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out DoomLikeFirstPersonController controller))
            {
                controller.Launch(GetLaunchVelocity(other.transform.position));
            }
        }
    }
}
