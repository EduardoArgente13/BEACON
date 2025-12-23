using UnityEngine;
using UnityEngine.Events;
using BEACON.Core.Interfaces;

namespace BEACON.Core.Health
{
    public class Health : MonoBehaviour, IDamageable
    {
        [Header("Stats")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float invulnerabilityDuration = 0.2f;

        [Header("Events")]
        public UnityEvent<float, float> OnHealthChanged; // current, max
        public UnityEvent<float> OnDamageTaken; // amount
        public UnityEvent OnDeath;

        public float CurrentHealth { get; private set; }
        public float MaxHealth => maxHealth;
        private float lastDamageTime;
        private bool isDead;

        private void Start()
        {
            CurrentHealth = maxHealth;
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        [SerializeField] private BEACON.Player.PlayerController playerController;

        public void TakeDamage(float amount, Vector2 knockback, GameObject source)
        {
            if (isDead) return;

            // If we have a PlayerController, let it handle the logic (Perfect Dodge, I-Frames, etc.)
            if (playerController != null)
            {
                // Assuming knockback force is magnitude of knockback vector or standard amount
                float knockbackForce = knockback.magnitude > 0 ? knockback.magnitude : 5f; 
                Vector2 knockbackDir = knockback.normalized;

                // PlayerController.TakeDamage returns TRUE if damage was taken, FALSE if negated
                bool damageApplied = playerController.TakeDamage(amount, knockbackDir, knockbackForce);
                
                if (!damageApplied) return; // Perfect Dodge or I-Frames kicked in
            }
            else
            {
                // Standard I-Frame check for non-player entities
                if (Time.time < lastDamageTime + invulnerabilityDuration) return;
            }

            CurrentHealth -= amount;
            lastDamageTime = Time.time;
            
            Debug.Log($"[Health] Took {amount} damage. Current HP: {CurrentHealth}/{maxHealth}");
            
            OnDamageTaken?.Invoke(amount);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

            if (CurrentHealth <= 0)
            {
                Die();
            }
        }

        private void Awake()
        {
            if (playerController == null) playerController = GetComponent<BEACON.Player.PlayerController>();
        }

        private void Die()
        {
            isDead = true;
            Debug.Log($"[Health] {gameObject.name} Has Died!");
            OnDeath?.Invoke();
            
            // Temporary simple death
            gameObject.SetActive(false);
        }

        public void Heal(float amount)
        {
            if (isDead) return;
            CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);
        }
    }
}
