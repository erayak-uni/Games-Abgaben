using System.Collections;
using UnityEngine;
using HL3.Combat;

namespace HL3.Player
{
    [RequireComponent(typeof(Damageable))]
    public sealed class PlayerRespawner : MonoBehaviour
    {
        [SerializeField] private Transform[] respawnPoints;
        [SerializeField] private float respawnDelay = 2f;

        private Damageable damageable;
        private CharacterController controller;
        private int nextPointIndex;

        private void Awake()
        {
            damageable = GetComponent<Damageable>();
            controller = GetComponent<CharacterController>();
        }

        private void OnEnable()
        {
            damageable.Died += OnDied;
        }

        private void OnDisable()
        {
            damageable.Died -= OnDied;
        }

        private void OnDied(DamagePayload payload)
        {
            StartCoroutine(RespawnRoutine());
        }

        private IEnumerator RespawnRoutine()
        {
            yield return new WaitForSeconds(respawnDelay);
            Transform point = GetNextPoint();
            if (controller != null)
            {
                controller.enabled = false;
            }

            damageable.Respawn(point.position, point.rotation);

            if (controller != null)
            {
                controller.enabled = true;
            }
        }

        private Transform GetNextPoint()
        {
            if (respawnPoints != null && respawnPoints.Length > 0)
            {
                Transform point = respawnPoints[nextPointIndex % respawnPoints.Length];
                nextPointIndex++;
                if (point != null)
                {
                    return point;
                }
            }

            return transform;
        }
    }
}
