using UnityEngine;

namespace HL3.Combat
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class ExplosiveProjectile : MonoBehaviour
    {
        [SerializeField] private float fuseSeconds = 2.2f;
        [SerializeField] private float radius = 5f;
        [SerializeField] private float damage = 55f;
        [SerializeField] private float knockback = 9f;
        [SerializeField] private LayerMask damageMask = ~0;
        [SerializeField] private GameObject explosionEffectPrefab;

        private GameObject instigator;
        private bool exploded;
        private Renderer cachedRenderer;

        public void SetInstigator(GameObject value)
        {
            instigator = value;
        }

        private void Awake()
        {
            cachedRenderer = GetComponent<Renderer>();
            if (cachedRenderer != null)
            {
                Material material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                material.color = new Color(0.08f, 0.08f, 0.08f);
                cachedRenderer.material = material;
            }

            if (GetComponent<TrailRenderer>() == null)
            {
                TrailRenderer trail = gameObject.AddComponent<TrailRenderer>();
                trail.time = 0.35f;
                trail.startWidth = 0.18f;
                trail.endWidth = 0f;
                trail.material = new Material(Shader.Find("Sprites/Default"));
                trail.startColor = new Color(1f, 0.6f, 0.1f);
                trail.endColor = new Color(1f, 0.2f, 0f, 0f);
            }
        }

        private void Start()
        {
            Invoke(nameof(Explode), fuseSeconds);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.relativeVelocity.magnitude > 10f)
            {
                Explode();
            }
        }

        private void Explode()
        {
            if (exploded)
            {
                return;
            }

            exploded = true;
            Vector3 center = transform.position;
            Collider[] hits = Physics.OverlapSphere(center, radius, damageMask, QueryTriggerInteraction.Ignore);
            foreach (Collider hit in hits)
            {
                Damageable damageable = hit.GetComponentInParent<Damageable>();
                if (damageable == null || !damageable.IsAlive)
                {
                    continue;
                }

                Vector3 closest = hit.ClosestPoint(center);
                float distance01 = Mathf.Clamp01(Vector3.Distance(center, closest) / radius);
                float scaledDamage = Mathf.Lerp(damage, damage * 0.25f, distance01);
                Vector3 direction = (closest - center).normalized;
                damageable.TakeDamage(new DamagePayload(scaledDamage, instigator, closest, direction, knockback));
            }

            if (explosionEffectPrefab != null)
            {
                Instantiate(explosionEffectPrefab, center, Quaternion.identity);
            }
            else
            {
                ExplosionVisual.Spawn(center, radius);
            }

            Destroy(gameObject);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.35f, 0.05f, 0.25f);
            Gizmos.DrawSphere(transform.position, radius);
        }
    }
}
