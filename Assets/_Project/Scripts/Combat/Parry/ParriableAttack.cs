using UnityEngine;

namespace BEACON.Combat.Parry
{
    public class ParriableAttack : MonoBehaviour, IParriable
    {
        [SerializeField] private float damage = 10f;
        [SerializeField] private ParryInteractionType interactionType = ParryInteractionType.Destructive;
        
        [Header("Reflection Settings")]
        [SerializeField] private float reflectedSpeedMultiplier = 1.5f;
        [SerializeField] private float reflectedDamageMultiplier = 2.0f;
        [SerializeField] private string enemyLayerName = "PlayerProjectile"; // Layer to hit after reflection

        public bool CanBeParried() => true;
        public ParryInteractionType InteractionType => interactionType;

        private bool isReflected = false;

        private void Start()
        {
            if (GetComponent<Rigidbody2D>() == null)
            {
                Debug.LogWarning($"[Parriable] '{gameObject.name}' missing Rigidbody2D! Collisions may not be detected. Add a Kinematic Rigidbody2D.");
            }
        }

        public float GetDamage() => damage;

        public GameObject GetSource() => gameObject;

        public void OnParried(ParryResult result)
        {
            Debug.Log($"[Parriable] I was parried! Result: {result.timing}");
            
            if (result.timing == ParryTiming.Perfect || result.timing == ParryTiming.Good)
            {
                switch (interactionType)
                {
                    case ParryInteractionType.Destructive:
                        // Destroy immediately
                        Destroy(gameObject);
                        break;

                    case ParryInteractionType.Reflective:
                        ReflectProjectile();
                        break;

                    case ParryInteractionType.Stun:
                        // Logic usually handled by local enemy controller via event, 
                        // but we can ensure visual feedback here if needed.
                        Debug.Log("[Parriable] Enemy Stunned!");
                        break;
                }
            }
        }

        private void ReflectProjectile()
        {
            isReflected = true;
            Debug.Log("[Parriable] REFLECTED!");
            
            // 1. Flip Direction & Boost Speed
            var rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Debug.Log($"[Parriable] Velocity Before: {rb.linearVelocity}");
                
                if (rb.linearVelocity.sqrMagnitude > 0.1f)
                {
                    rb.linearVelocity = -rb.linearVelocity * reflectedSpeedMultiplier;
                }
                else
                {
                     // Fallback if stationary but has RB
                    Debug.Log("[Parriable] Velocity was zero, applying Kickback Force based on rotation.");
                    rb.linearVelocity = -transform.right * (5f * reflectedSpeedMultiplier);
                }

                Debug.Log($"[Parriable] Velocity After: {rb.linearVelocity}");

                // Rotate visual to match new velocity
                if (rb.linearVelocity.sqrMagnitude > 0.01f)
                {
                    float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                }
            }
            else
            {
                // FALLBACK: Transform-based movement (e.g. SimpleProjectile script)
                Debug.Log("[Parriable] No Rigidbody found! Reflecting via Transform Rotation.");
                
                // Flip 180 degrees around Z axis
                transform.Rotate(0f, 0f, 180f);
                
                // Note: SimpleProjectile uses 'speed', which we can't easily modify generically without GetComponent.
                // But rotating it 180 will send it back if it uses 'transform.Translate(Vector2.right)'
                
                // Optional: Try to boost speed if it's the specific test script
                var simpleProj = GetComponent<BEACON.Testing.SimpleProjectile>();
                // We can't modify 'speed' because it's private serializable, unless we expose it or use reflection.
                // For now, direction flip is enough to prove it works.
            }

            // 2. Increase Damage
            damage *= reflectedDamageMultiplier;

            // 3. Change Layer/Tag/Faction so it hits enemies
            int targetLayer = LayerMask.NameToLayer(enemyLayerName);
            
            if (targetLayer != -1)
            {
                gameObject.layer = targetLayer;
            }
            else
            {
                Debug.LogWarning($"[Parriable] Layer '{enemyLayerName}' not found! Please create it in Project Settings > Tags and Layers. Projectile will keep current layer.");
            }
            
            // 4. Reset Lifetime (optional, if using a timer script)
            // Send message to reset timer?
            // BroadcastMessage("ResetLifetime", SendMessageOptions.DontRequireReceiver);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Debug.Log($"[Parriable] Hit: {other.name} (Tag: {other.tag}, Layer: {other.gameObject.layer})");

            var damageable = other.GetComponent<BEACON.Core.Interfaces.IDamageable>();
            if (damageable != null)
            {
                // Logic:
                // If Reflected: Should damage Enemies.
                // If Normal: Should damage Player.
                
                // Safely check tags
                bool isPlayer = SafeCompareTag(other, "Player");
                bool isEnemy = SafeCompareTag(other, "Enemy");

                if (isReflected && isPlayer) return; // Don't hurt player if reflected
                if (!isReflected && isEnemy) return; // Don't hurt enemy if not reflected (Friendly fire?)

                Debug.Log($"[Parriable] [{Time.time:F2}] Applying Damage to {other.name} (Reflected: {isReflected})");
                
                // Calculate knockback
                Vector2 direction = (other.transform.position - transform.position).normalized;
                damageable.TakeDamage(damage, direction * 5f, gameObject);
                
                Destroy(gameObject);
            }
        }

        private bool SafeCompareTag(Component other, string tag)
        {
            try
            {
                return other.CompareTag(tag);
            }
            catch
            {
                Debug.LogWarning($"[Parriable] Tag '{tag}' is not defined in Project Settings! treating as false.");
                return false;
            }
        }
    }
}
