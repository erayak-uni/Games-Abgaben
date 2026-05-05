using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using HL3.Combat;

namespace HL3.AI
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Damageable))]
    public sealed class EnemyBot : MonoBehaviour
    {
        [Header("Targeting")]
        [SerializeField] private Transform target;
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private LayerMask sightBlockers = ~0;
        [SerializeField] private float awarenessDistance = 28f;
        [SerializeField] private float sightDistance = 24f;
        [SerializeField] private float fieldOfView = 110f;
        [SerializeField] private float attackDistance = 2.2f;
        [SerializeField] private float preferredCombatDistance = 10f;
        [SerializeField] private float memorySeconds = 3f;

        [Header("Combat")]
        [SerializeField] private float attackDamage = 12f;
        [SerializeField] private float attackCooldown = 1.1f;
        [SerializeField] private bool useRangedAttack = true;
        [SerializeField] private float rangedAttackDistance = 18f;
        [SerializeField] private float projectileSpeed = 20f;
        [SerializeField] private Color projectileColor = new Color(1f, 0.45f, 0.05f);
        [SerializeField] private float respawnDelay = 6f;
        [SerializeField] private bool respawnAfterDeath = true;

        [Header("Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private string speedFloat = "Speed";
        [SerializeField] private string attackTrigger = "Attack";
        [SerializeField] private string hitTrigger = "Hit";
        [SerializeField] private string deadBool = "Dead";

        private NavMeshAgent agent;
        private Damageable damageable;
        private Vector3 spawnPosition;
        private Quaternion spawnRotation;
        private float lastSeenTime = -999f;
        private float nextAttackTime;
        private bool isDead;

        public event Action AttackStarted;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            damageable = GetComponent<Damageable>();
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            spawnPosition = transform.position;
            spawnRotation = transform.rotation;
        }

        private void Start()
        {
            PlaceOnNavMeshIfNeeded();
        }

        private void OnEnable()
        {
            damageable.Damaged += OnDamaged;
            damageable.Died += OnDied;
            damageable.Respawned += OnRespawned;
        }

        private void OnDisable()
        {
            damageable.Damaged -= OnDamaged;
            damageable.Died -= OnDied;
            damageable.Respawned -= OnRespawned;
        }

        private void Update()
        {
            if (isDead)
            {
                return;
            }

            AcquireTargetIfNeeded();
            bool seesTarget = CanSeeTarget();
            if (seesTarget)
            {
                lastSeenTime = Time.time;
            }

            bool targetInAwareness = IsTargetInAwarenessRange();
            bool shouldEngage = target != null && (targetInAwareness || Time.time - lastSeenTime <= memorySeconds);
            if (shouldEngage)
            {
                float distance = Vector3.Distance(transform.position, target.position);
                if (seesTarget && useRangedAttack && distance <= rangedAttackDistance)
                {
                    if (distance > preferredCombatDistance)
                    {
                        MoveTowardTarget();
                    }
                    else
                    {
                        HoldPositionAndFaceTarget();
                    }

                    TryRangedAttack();
                }
                else if (distance <= attackDistance && seesTarget)
                {
                    Attack();
                }
                else
                {
                    MoveTowardTarget();
                }
            }
            else
            {
                agent.isStopped = true;
            }

            UpdateAnimator();
        }

        private void AcquireTargetIfNeeded()
        {
            if (target != null)
            {
                return;
            }

            GameObject player = GameObject.FindGameObjectWithTag(playerTag);
            if (player != null)
            {
                target = player.transform;
            }
        }

        private void PlaceOnNavMeshIfNeeded()
        {
            if (agent == null)
            {
                return;
            }

            if (agent.isOnNavMesh)
            {
                return;
            }

            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 4f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
            }
        }

        private bool CanSeeTarget()
        {
            if (target == null)
            {
                return false;
            }

            Vector3 eye = transform.position + Vector3.up * 1.55f;
            Vector3 targetPoint = target.position + Vector3.up * 1.2f;
            Vector3 toTarget = targetPoint - eye;
            if (toTarget.magnitude > sightDistance)
            {
                return false;
            }

            if (Vector3.Angle(transform.forward, toTarget) > fieldOfView * 0.5f)
            {
                return false;
            }

            if (Physics.Raycast(eye, toTarget.normalized, out RaycastHit hit, sightDistance, sightBlockers, QueryTriggerInteraction.Ignore))
            {
                return hit.transform == target || hit.transform.IsChildOf(target);
            }

            return true;
        }

        private bool IsTargetInAwarenessRange()
        {
            if (target == null)
            {
                return false;
            }

            return Vector3.Distance(transform.position, target.position) <= awarenessDistance;
        }

        private void Attack()
        {
            agent.isStopped = true;
            Vector3 lookDirection = target.position - transform.position;
            lookDirection.y = 0f;
            if (lookDirection.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(lookDirection), 720f * Time.deltaTime);
            }

            if (Time.time < nextAttackTime)
            {
                return;
            }

            nextAttackTime = Time.time + attackCooldown;
            if (animator != null && !string.IsNullOrWhiteSpace(attackTrigger))
            {
                animator.SetTrigger(attackTrigger);
            }
            AttackStarted?.Invoke();

            Damageable damageableTarget = target.GetComponentInParent<Damageable>();
            if (damageableTarget != null)
            {
                Vector3 direction = (target.position - transform.position).normalized;
                damageableTarget.TakeDamage(new DamagePayload(attackDamage, gameObject, target.position, direction, 1.5f));
            }
        }

        private void TryRangedAttack()
        {
            if (!useRangedAttack || Time.time < nextAttackTime)
            {
                return;
            }

            nextAttackTime = Time.time + attackCooldown;
            if (animator != null && !string.IsNullOrWhiteSpace(attackTrigger))
            {
                animator.SetTrigger(attackTrigger);
            }
            AttackStarted?.Invoke();

            Vector3 origin = transform.position + Vector3.up * 1.45f + transform.forward * 0.55f;
            Vector3 targetPoint = target.position + Vector3.up * 1.1f;
            Vector3 direction = (targetPoint - origin).normalized;
            EnemyProjectile projectile = CreateProjectile(origin);
            projectile.Launch(gameObject, direction, attackDamage, projectileSpeed);
        }

        private void MoveTowardTarget()
        {
            if (!agent.isOnNavMesh)
            {
                PlaceOnNavMeshIfNeeded();
                if (!agent.isOnNavMesh)
                {
                    return;
                }
            }

            agent.isStopped = false;
            agent.SetDestination(target.position);
            FaceTargetFlat();
        }

        private void HoldPositionAndFaceTarget()
        {
            if (!agent.isOnNavMesh)
            {
                PlaceOnNavMeshIfNeeded();
            }

            agent.isStopped = true;
            FaceTargetFlat();
        }

        private void FaceTargetFlat()
        {
            if (target == null)
            {
                return;
            }

            Vector3 lookDirection = target.position - transform.position;
            lookDirection.y = 0f;
            if (lookDirection.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(lookDirection), 720f * Time.deltaTime);
            }
        }

        private EnemyProjectile CreateProjectile(Vector3 origin)
        {
            GameObject projectileObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectileObject.name = "Enemy_Energy_Shot";
            projectileObject.transform.position = origin;
            projectileObject.transform.localScale = Vector3.one * 0.28f;

            SphereCollider collider = projectileObject.GetComponent<SphereCollider>();
            collider.isTrigger = true;

            Rigidbody rb = projectileObject.AddComponent<Rigidbody>();
            rb.useGravity = false;

            Renderer renderer = projectileObject.GetComponent<Renderer>();
            Material material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            material.color = projectileColor;
            renderer.material = material;

            TrailRenderer trail = projectileObject.AddComponent<TrailRenderer>();
            trail.time = 0.18f;
            trail.startWidth = 0.16f;
            trail.endWidth = 0f;
            trail.material = new Material(Shader.Find("Sprites/Default"));
            trail.startColor = projectileColor;
            trail.endColor = new Color(projectileColor.r, projectileColor.g, projectileColor.b, 0f);

            return projectileObject.AddComponent<EnemyProjectile>();
        }

        private void OnDamaged(DamagePayload payload)
        {
            if (animator != null && !string.IsNullOrWhiteSpace(hitTrigger))
            {
                animator.SetTrigger(hitTrigger);
            }

            if (payload.Instigator != null)
            {
                target = payload.Instigator.transform;
                lastSeenTime = Time.time;
            }
        }

        private void OnDied(DamagePayload payload)
        {
            isDead = true;
            agent.isStopped = true;
            agent.enabled = false;

            if (animator != null && !string.IsNullOrWhiteSpace(deadBool))
            {
                animator.SetBool(deadBool, true);
            }

            if (respawnAfterDeath)
            {
                StartCoroutine(RespawnRoutine());
            }
        }

        private void OnRespawned()
        {
            isDead = false;
            agent.enabled = true;
            agent.Warp(transform.position);
            agent.isStopped = false;
            if (animator != null && !string.IsNullOrWhiteSpace(deadBool))
            {
                animator.SetBool(deadBool, false);
            }
        }

        private IEnumerator RespawnRoutine()
        {
            yield return new WaitForSeconds(respawnDelay);
            damageable.Respawn(spawnPosition, spawnRotation);
        }

        private void UpdateAnimator()
        {
            if (animator == null || string.IsNullOrWhiteSpace(speedFloat))
            {
                return;
            }

            float speed01 = agent.enabled ? agent.velocity.magnitude / Mathf.Max(agent.speed, 0.01f) : 0f;
            animator.SetFloat(speedFloat, speed01, 0.1f, Time.deltaTime);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, sightDistance);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackDistance);
        }
    }
}
