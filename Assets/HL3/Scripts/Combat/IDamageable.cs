using UnityEngine;

namespace HL3.Combat
{
    public interface IDamageable
    {
        bool IsAlive { get; }
        void TakeDamage(DamagePayload payload);
    }

    public readonly struct DamagePayload
    {
        public readonly float Amount;
        public readonly GameObject Instigator;
        public readonly Vector3 Point;
        public readonly Vector3 Direction;
        public readonly float Knockback;

        public DamagePayload(float amount, GameObject instigator, Vector3 point, Vector3 direction, float knockback = 0f)
        {
            Amount = amount;
            Instigator = instigator;
            Point = point;
            Direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.zero;
            Knockback = knockback;
        }
    }
}
