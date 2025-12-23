using UnityEngine;
using BEACON.Core.Interfaces;

namespace BEACON.Combat.Projectiles
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class PlayerProjectile : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float damage = 15f;
        [SerializeField] private float lifeTime = 5f;
        
        [Header("VFX")]
        [SerializeField] private GameObject hitEffectPrefab;

        [Header("Filter Settings")]
        [SerializeField] private string[] targetTags = { "Enemy", "Boss" };
        [SerializeField] private bool destroyOnAnySurface = true;

        public void Initialize(float damageAmount, Vector2 knockback)
        {
            damage = damageAmount;
            // You could also store knockback if needed
        }

        private void Start()
        {
            Destroy(gameObject, lifeTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // 1. Physical Layer Check (Ignore Player/Self)
            if (other.CompareTag("Player")) return;

            // 2. Filter targets (Enemies, Bosses, etc.)
            bool isTarget = false;
            foreach (string tag in targetTags)
            {
                if (other.CompareTag(tag))
                {
                    isTarget = true;
                    break;
                }
            }

            if (isTarget)
            {
                var damageable = other.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    Vector2 knockbackDir = GetComponent<Rigidbody2D>().linearVelocity.normalized;
                    damageable.TakeDamage(damage, knockbackDir, gameObject);
                    Hit();
                    return;
                }
            }

            // 3. Environmental Collision (Walls, Floors, etc.)
            // We only destroy on NON-TRIGGERS to avoid accidental destruction on pickup zones/save points
            if (destroyOnAnySurface && !other.isTrigger) 
            {
                Hit();
            }
        }

        private void Hit()
        {
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            }
            Destroy(gameObject);
        }
    }
}
