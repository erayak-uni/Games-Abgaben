using System.Collections;
using HL3.Combat;
using UnityEngine;
using UnityEngine.AI;

namespace HL3.AI
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Damageable))]
    [RequireComponent(typeof(EnemyBot))]
    public sealed class SimpleBotAnimator : MonoBehaviour
    {
        [Header("Visual Parts")]
        [SerializeField] private Transform body;
        [SerializeField] private Renderer bodyRenderer;

        [Header("Colors")]
        [SerializeField] private Color idleColor = new Color(1f, 0.72f, 0.12f);
        [SerializeField] private Color hitColor = Color.red;
        [SerializeField] private Color deadColor = new Color(0.15f, 0.15f, 0.15f);

        [Header("Motion")]
        [SerializeField] private float idleBobHeight = 0.08f;
        [SerializeField] private float walkBobHeight = 0.18f;
        [SerializeField] private float walkLeanDegrees = 7f;
        [SerializeField] private float attackPunchDistance = 0.35f;

        private NavMeshAgent agent;
        private Damageable damageable;
        private EnemyBot enemyBot;
        private Vector3 baseLocalPosition;
        private Quaternion baseLocalRotation;
        private Material runtimeMaterial;
        private float attackTimer;
        private float hitTimer;
        private bool dead;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            damageable = GetComponent<Damageable>();
            enemyBot = GetComponent<EnemyBot>();

            if (body == null)
            {
                body = transform;
            }

            if (bodyRenderer == null)
            {
                bodyRenderer = GetComponentInChildren<Renderer>();
            }

            if (bodyRenderer != null)
            {
                runtimeMaterial = bodyRenderer.material;
                runtimeMaterial.color = idleColor;
            }

            baseLocalPosition = body.localPosition;
            baseLocalRotation = body.localRotation;
        }

        private void OnEnable()
        {
            damageable.Damaged += OnDamaged;
            damageable.Died += OnDied;
            damageable.Respawned += OnRespawned;
            enemyBot.AttackStarted += OnAttackStarted;
        }

        private void OnDisable()
        {
            damageable.Damaged -= OnDamaged;
            damageable.Died -= OnDied;
            damageable.Respawned -= OnRespawned;
            enemyBot.AttackStarted -= OnAttackStarted;
        }

        private void Update()
        {
            if (dead)
            {
                return;
            }

            attackTimer = Mathf.Max(0f, attackTimer - Time.deltaTime);
            hitTimer = Mathf.Max(0f, hitTimer - Time.deltaTime);

            float speed = agent.enabled ? agent.velocity.magnitude : 0f;
            bool walking = speed > 0.15f;
            float bobHeight = walking ? walkBobHeight : idleBobHeight;
            float bobSpeed = walking ? 11f : 3f;
            float bob = Mathf.Sin(Time.time * bobSpeed) * bobHeight;

            Vector3 punch = Vector3.zero;
            if (attackTimer > 0f)
            {
                punch = Vector3.forward * (Mathf.Sin((1f - attackTimer / 0.28f) * Mathf.PI) * attackPunchDistance);
            }

            body.localPosition = baseLocalPosition + Vector3.up * bob + punch;
            body.localRotation = baseLocalRotation * Quaternion.Euler(walking ? Mathf.Sin(Time.time * 9f) * walkLeanDegrees : 0f, 0f, 0f);

            if (runtimeMaterial != null)
            {
                runtimeMaterial.color = hitTimer > 0f ? hitColor : idleColor;
            }
        }

        private void OnAttackStarted()
        {
            attackTimer = 0.28f;
        }

        private void OnDamaged(DamagePayload payload)
        {
            hitTimer = 0.18f;
        }

        private void OnDied(DamagePayload payload)
        {
            dead = true;
            if (runtimeMaterial != null)
            {
                runtimeMaterial.color = deadColor;
            }

            StopAllCoroutines();
            StartCoroutine(DeathPoseRoutine());
        }

        private void OnRespawned()
        {
            dead = false;
            body.localPosition = baseLocalPosition;
            body.localRotation = baseLocalRotation;
            if (runtimeMaterial != null)
            {
                runtimeMaterial.color = idleColor;
            }
        }

        private IEnumerator DeathPoseRoutine()
        {
            Quaternion from = body.localRotation;
            Quaternion to = baseLocalRotation * Quaternion.Euler(90f, 0f, 0f);
            float timer = 0f;
            while (timer < 0.35f)
            {
                timer += Time.deltaTime;
                body.localRotation = Quaternion.Slerp(from, to, timer / 0.35f);
                yield return null;
            }
        }
    }
}
