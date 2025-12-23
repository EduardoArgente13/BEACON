using UnityEngine;

namespace BEACON.Core.Interfaces
{
    public interface IDamageable
    {
        void TakeDamage(float amount, Vector2 knockback, GameObject source);
    }
}
