using System;
using UnityEngine;

namespace HL3.Combat
{
    public sealed class Damageable : MonoBehaviour, IDamageable
    {
        [Header("Health")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float invulnerableSecondsAfterHit = 0.05f;
        [SerializeField] private bool disableRenderersAndCollidersOnDeath = false;

        [Header("Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private string hitTrigger = "Hit";
        [SerializeField] private string deadBool = "Dead";

        public event Action<DamagePayload> Damaged;
        public event Action<DamagePayload> Died;
        public event Action Respawned;

        private float currentHealth;
        private float nextDamageTime;
        private Collider[] cachedColliders;
        private Renderer[] cachedRenderers;
        private bool isAlive = true;

        public bool IsAlive => isAlive;
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public float NormalizedHealth => maxHealth <= 0f ? 0f : currentHealth / maxHealth;

        private void Awake()
        {
            currentHealth = maxHealth;
            cachedColliders = GetComponentsInChildren<Collider>(true);
            cachedRenderers = GetComponentsInChildren<Renderer>(true);
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }

        public void TakeDamage(DamagePayload payload)
        {
            if (!isAlive || Time.time < nextDamageTime || payload.Amount <= 0f)
            {
                return;
            }

            nextDamageTime = Time.time + invulnerableSecondsAfterHit;
            currentHealth = Mathf.Max(0f, currentHealth - payload.Amount);

            if (animator != null && !string.IsNullOrWhiteSpace(hitTrigger))
            {
                animator.SetTrigger(hitTrigger);
            }

            Damaged?.Invoke(payload);

            if (currentHealth <= 0f)
            {
                Die(payload);
            }
        }

        public void Respawn(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);
            currentHealth = maxHealth;
            isAlive = true;
            nextDamageTime = 0f;

            SetScenePresence(true);
            if (animator != null && !string.IsNullOrWhiteSpace(deadBool))
            {
                animator.SetBool(deadBool, false);
            }

            Respawned?.Invoke();
        }

        private void Die(DamagePayload payload)
        {
            isAlive = false;
            if (animator != null && !string.IsNullOrWhiteSpace(deadBool))
            {
                animator.SetBool(deadBool, true);
            }

            Died?.Invoke(payload);

            if (disableRenderersAndCollidersOnDeath)
            {
                SetScenePresence(false);
            }
        }

        private void SetScenePresence(bool active)
        {
            foreach (Collider col in cachedColliders)
            {
                if (col != null)
                {
                    col.enabled = active;
                }
            }

            foreach (Renderer renderer in cachedRenderers)
            {
                if (renderer != null)
                {
                    renderer.enabled = active;
                }
            }
        }
    }
}
