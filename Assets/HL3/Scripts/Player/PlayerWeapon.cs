using UnityEngine;
using UnityEngine.InputSystem;
using HL3.Combat;

namespace HL3.Player
{
    public sealed class PlayerWeapon : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform cameraRoot;
        [SerializeField] private Transform muzzle;
        [SerializeField] private ExplosiveProjectile bombPrefab;
        [SerializeField] private LayerMask hitMask = ~0;

        [Header("Hitscan")]
        [SerializeField] private float damage = 20f;
        [SerializeField] private float range = 80f;
        [SerializeField] private float knockback = 3f;
        [SerializeField] private float fireCooldown = 0.12f;
        [SerializeField] private float visualBulletSpeed = 70f;
        [SerializeField] private Color visualBulletColor = Color.white;

        [Header("Bomb")]
        [SerializeField] private float bombThrowForce = 18f;
        [SerializeField] private float bombCooldown = 1.2f;

        private float nextFireTime;
        private float nextBombTime;

        private void Awake()
        {
            if (cameraRoot == null)
            {
                Camera childCamera = GetComponentInChildren<Camera>();
                if (childCamera != null)
                {
                    cameraRoot = childCamera.transform;
                }
                else if (Camera.main != null)
                {
                    cameraRoot = Camera.main.transform;
                }
            }
        }

        private void Update()
        {
            if (Mouse.current == null || Keyboard.current == null || cameraRoot == null)
            {
                return;
            }

            if (Mouse.current.leftButton.isPressed && Time.time >= nextFireTime)
            {
                FireHitscan();
            }

            if (Keyboard.current.qKey.wasPressedThisFrame && Time.time >= nextBombTime)
            {
                ThrowBomb();
            }
        }

        private void FireHitscan()
        {
            nextFireTime = Time.time + fireCooldown;
            Ray ray = new Ray(cameraRoot.position, cameraRoot.forward);
            Vector3 visualEndPoint = ray.origin + ray.direction * range;
            if (Physics.Raycast(ray, out RaycastHit hit, range, hitMask, QueryTriggerInteraction.Ignore))
            {
                visualEndPoint = hit.point;
                Damageable damageable = hit.collider.GetComponentInParent<Damageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(new DamagePayload(damage, gameObject, hit.point, ray.direction, knockback));
                }
            }

            SpawnVisualBullet(ray.origin + ray.direction * 0.55f, visualEndPoint);
        }

        private void SpawnVisualBullet(Vector3 start, Vector3 end)
        {
            GameObject bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bullet.name = "Player_Visual_Bullet";
            bullet.transform.position = start;
            bullet.transform.localScale = Vector3.one * 0.12f;

            Collider collider = bullet.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            Renderer renderer = bullet.GetComponent<Renderer>();
            Material material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            material.color = visualBulletColor;
            renderer.material = material;

            TrailRenderer trail = bullet.AddComponent<TrailRenderer>();
            trail.time = 0.12f;
            trail.startWidth = 0.08f;
            trail.endWidth = 0f;
            trail.material = new Material(Shader.Find("Sprites/Default"));
            trail.startColor = visualBulletColor;
            trail.endColor = new Color(visualBulletColor.r, visualBulletColor.g, visualBulletColor.b, 0f);

            VisualBullet visual = bullet.AddComponent<VisualBullet>();
            visual.Launch(start, end, visualBulletSpeed);
        }

        private sealed class VisualBullet : MonoBehaviour
        {
            private Vector3 target;
            private float speed;

            public void Launch(Vector3 start, Vector3 end, float bulletSpeed)
            {
                transform.position = start;
                target = end;
                speed = bulletSpeed;
                Destroy(gameObject, 0.6f);
            }

            private void Update()
            {
                transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
                if (Vector3.Distance(transform.position, target) <= 0.05f)
                {
                    Destroy(gameObject);
                }
            }
        }

        private void ThrowBomb()
        {
            nextBombTime = Time.time + bombCooldown;
            Vector3 spawnPosition = muzzle != null ? muzzle.position : cameraRoot.position + cameraRoot.forward * 0.6f;
            ExplosiveProjectile projectile = bombPrefab != null
                ? Instantiate(bombPrefab, spawnPosition, Quaternion.LookRotation(cameraRoot.forward))
                : CreateRuntimeBomb(spawnPosition, Quaternion.LookRotation(cameraRoot.forward));

            projectile.SetInstigator(gameObject);

            if (projectile.TryGetComponent(out Rigidbody rb))
            {
                rb.linearVelocity = cameraRoot.forward * bombThrowForce + Vector3.up * 2f;
            }
        }

        private ExplosiveProjectile CreateRuntimeBomb(Vector3 position, Quaternion rotation)
        {
            GameObject bomb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bomb.name = "HL3_Runtime_Bomb";
            bomb.transform.SetPositionAndRotation(position, rotation);
            bomb.transform.localScale = Vector3.one * 0.45f;

            Rigidbody rb = bomb.AddComponent<Rigidbody>();
            rb.mass = 1.2f;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            return bomb.AddComponent<ExplosiveProjectile>();
        }
    }
}
