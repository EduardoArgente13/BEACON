using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

namespace BEACON.Combat
{
    /// <summary>
    /// Manages hitbox detection for combat using Physics2D Overlap methods.
    /// Supports Boxes, Circles, and Capsules. Handles visualization and hit filtering.
    /// </summary>
    public class HitboxController : MonoBehaviour
    {
        // ============ EVENTS ============
        
        [System.Serializable]
        public class HitEvent : UnityEvent<GameObject, Vector2, float> { }
        public HitEvent OnHitConfirmed; // Target, HitPoint, Damage

        // ============ CONFIGURATION ============
        
        [Header("Settings")]
        [SerializeField] private LayerMask hittableLayers;
        [SerializeField] private bool showDebugGizmos = true;
        
        // ============ STATE ============
        
        private HitboxShape currentShape;
        private Vector2 currentOffset;
        private Vector2 currentSize; // x=width/radius, y=height
        private bool isHitboxActive;
        
        private List<Collider2D> hitHistory = new List<Collider2D>();
        private ContactFilter2D contactFilter;
        private Collider2D[] overlapResults = new Collider2D[10]; // Max 10 hits per frame
        
        // Current attack data for context
        private float currentDamage;
        private Vector2 currentKnockback;

        // ============ INITIALIZATION ============

        private void Awake()
        {
            contactFilter = new ContactFilter2D();
            contactFilter.SetLayerMask(hittableLayers);
            contactFilter.useTriggers = true; // Hit triggers too (enemy hitboxes)
        }

        // ============ UPDATE ============

        private void FixedUpdate()
        {
            if (isHitboxActive)
            {
                CheckHits();
            }
        }

        // ============ PUBLIC METHODS ============

        /// <summary>
        /// Configures and enables the hitbox for a specific frame or duration.
        /// </summary>
        public void ActivateHitbox(HitboxShape shape, Vector2 offset, Vector2 size, float damage, Vector2 knockback)
        {
            currentShape = shape;
            currentOffset = offset;
            currentSize = size;
            currentDamage = damage;
            currentKnockback = knockback;
            
            isHitboxActive = true;
            hitHistory.Clear(); // New swing = fresh hit history
        }

        /// <summary>
        /// Disables the hitbox immediately.
        /// </summary>
        public void DeactivateHitbox()
        {
            isHitboxActive = false;
        }

        // ============ DETECTION LOGIC ============

        private void CheckHits()
        {
            // Adjust offset based on parent facing (assuming local X is forward)
            // Note: This requires the parent transform to handle flipping via Scale.x
            Vector2 worldPos = (Vector2)transform.position + new Vector2(
                currentOffset.x * Mathf.Sign(transform.lossyScale.x), 
                currentOffset.y
            );
            
            int hitCount = 0;

            switch (currentShape)
            {
                case HitboxShape.Box:
                    hitCount = Physics2D.OverlapBox(worldPos, currentSize, 0f, contactFilter, overlapResults);
                    break;
                    
                case HitboxShape.Circle:
                    hitCount = Physics2D.OverlapCircle(worldPos, currentSize.x, contactFilter, overlapResults);
                    break;
                    
                case HitboxShape.Capsule:
                    hitCount = Physics2D.OverlapCapsule(worldPos, currentSize, CapsuleDirection2D.Horizontal, 0f, contactFilter, overlapResults);
                    break;
            }

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D hitCol = overlapResults[i];
                
                // Ignore self if collider is on us
                if (hitCol.transform.root == transform.root) continue;
                
                // Ignore if already hit in this swing (prevents multi-hit in one frame)
                if (hitHistory.Contains(hitCol)) continue;
                
                ProcessHit(hitCol);
            }
        }

        private void ProcessHit(Collider2D target)
        {
            hitHistory.Add(target);

            // TODO: Apply damage via Interface (IDamageable)
            Debug.Log($"[Hitbox] Hit {target.name} for {currentDamage} damage!");

            // Calculate hit point (approximate)
            Vector2 hitPoint = target.ClosestPoint(transform.position);

            OnHitConfirmed?.Invoke(target.gameObject, hitPoint, currentDamage);
        }

        // ============ DEBUG ============

        private void OnDrawGizmos()
        {
            if (!showDebugGizmos) return;

            // Draw inactive (yellow) or active (red)
            Gizmos.color = isHitboxActive ? new Color(1, 0, 0, 0.5f) : new Color(1, 1, 0, 0.2f);
            
             Vector2 worldPos = (Vector2)transform.position + new Vector2(
                currentOffset.x * Mathf.Sign(transform.lossyScale.x), 
                currentOffset.y
            );

            switch (currentShape)
            {
                case HitboxShape.Box:
                    Gizmos.DrawCube(worldPos, currentSize);
                    Gizmos.DrawWireCube(worldPos, currentSize);
                    break;
                    
                case HitboxShape.Circle:
                    Gizmos.DrawSphere(worldPos, currentSize.x);
                    Gizmos.DrawWireSphere(worldPos, currentSize.x);
                    break;
                    
                case HitboxShape.Capsule:
                    // Gizmos doesn't have DrawCapsule, approximating with wire sphere
                    Gizmos.DrawWireSphere(worldPos, Mathf.Max(currentSize.x, currentSize.y) / 2f);
                    break;
            }
        }
    }
}
