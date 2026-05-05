using UnityEngine;

namespace HL3.Combat
{
    [RequireComponent(typeof(SphereCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class EnemyProjectile : MonoBehaviour
    {
        [SerializeField] private float damage = 10f;
        [SerializeField] private float speed = 18f;
        [SerializeField] private float lifeSeconds = 4f;
        [SerializeField] private float knockback = 1.5f;

        private GameObject instigator;
        private Vector3 direction;

        public void Launch(GameObject owner, Vector3 launchDirection, float projectileDamage, float projectileSpeed)
        {
            instigator = owner;
            direction = launchDirection.sqrMagnitude > 0.001f ? launchDirection.normalized : transform.forward;
            damage = projectileDamage;
            speed = projectileSpeed;
            transform.rotation = Quaternion.LookRotation(direction);

            Rigidbody rb = GetComponent<Rigidbody>();
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.linearVelocity = direction * speed;

            Destroy(gameObject, lifeSeconds);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (instigator != null && other.transform.IsChildOf(instigator.transform))
            {
                return;
            }

            Damageable damageable = other.GetComponentInParent<Damageable>();
            if (damageable != null && damageable.IsAlive)
            {
                damageable.TakeDamage(new DamagePayload(damage, instigator, transform.position, direction, knockback));
            }

            Destroy(gameObject);
        }
    }
}
