using UnityEngine;
using System.Collections;

namespace BEACON.Testing
{
    public class DummyAttacker : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private GameObject attackPrefab; // Should have ParriableAttack component
        [SerializeField] private float attackInterval = 2f;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private float telegraphTime = 0.5f;

        private void Start()
        {
            StartCoroutine(AttackLoop());
        }

        private IEnumerator AttackLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(attackInterval - telegraphTime);
                
                // Telegraph (Flash red or something)
                GetComponent<SpriteRenderer>().color = Color.red;
                yield return new WaitForSeconds(telegraphTime);
                
                // Attack
                GetComponent<SpriteRenderer>().color = Color.white;
                SpawnAttack();
            }
        }

        private void SpawnAttack()
        {
            if (attackPrefab != null)
            {
                // Determine direction based on attacker's facing (handles scale flips)
                Vector2 forward = transform.right; 
                float angle = Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg;
                
                GameObject proj = Instantiate(attackPrefab, spawnPoint.position, Quaternion.Euler(0, 0, angle));
                
                // If it has a Rigidbody, we should also give it velocity just like the player
                var rb = proj.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    // Assuming a default speed if it doesn't have its own movement script
                    rb.linearVelocity = forward * 10f; 
                }
                
                Debug.Log($"[DummyAttacker] Spawned attack. Direction: {forward}");
            }
            else
            {
                Debug.LogWarning("[DummyAttacker] No Attack Prefab assigned!");
            }
        }
    }
}
