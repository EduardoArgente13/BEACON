using UnityEngine;

namespace BEACON.Player
{
    /// <summary>
    /// Robust ground detection using multiple raycasts.
    /// Handles slopes, platform edges, and provides ground normal for physics.
    /// </summary>
    public class GroundChecker : MonoBehaviour
    {
        // ============ CONFIGURATION ============
        
        [Header("Ground Detection")]
        [SerializeField, Tooltip("Layer(s) considered as ground")]
        private LayerMask groundLayer;

        [SerializeField, Tooltip("Transform at the player's feet (origin of raycasts)")]
        private Transform feetPosition;

        [SerializeField, Tooltip("Length of ground check raycasts")]
        private float raycastDistance = 0.1f;

        [SerializeField, Tooltip("Horizontal offset for side raycasts from center")]
        private float sideRayOffset = 0.4f;

        [SerializeField, Tooltip("Maximum slope angle player can stand on")]
        private float maxSlopeAngle = 45f;

        [Header("Coyote Time Support")]
        [SerializeField, Tooltip("Small buffer to consider recently left ground")]
        private float groundedBufferTime = 0.05f;

        [Header("Debug")]
        [SerializeField]
        private bool showDebugGizmos = true;

        [SerializeField]
        private Color groundedColor = Color.green;

        [SerializeField]
        private Color airborneColor = Color.red;

        // ============ STATE ============
        
        private bool isGrounded;
        private bool wasGroundedLastFrame;
        private float lastGroundedTime;
        private Vector2 groundNormal;
        private float slopeAngle;
        private RaycastHit2D centerHit;
        private RaycastHit2D leftHit;
        private RaycastHit2D rightHit;

        // ============ PUBLIC PROPERTIES ============

        /// <summary>True if any ground raycast is hitting</summary>
        public bool IsGrounded => isGrounded;

        /// <summary>True if grounded or within buffer time (for coyote time)</summary>
        public bool IsGroundedBuffered => isGrounded || (Time.time - lastGroundedTime < groundedBufferTime);

        /// <summary>True if player just landed this frame</summary>
        public bool JustLanded => isGrounded && !wasGroundedLastFrame;

        /// <summary>True if player just left ground this frame</summary>
        public bool JustLeftGround => !isGrounded && wasGroundedLastFrame;

        /// <summary>Normal vector of the ground surface</summary>
        public Vector2 GroundNormal => groundNormal;

        /// <summary>Current slope angle in degrees</summary>
        public float SlopeAngle => slopeAngle;

        /// <summary>True if standing on a walkable slope</summary>
        public bool IsOnSlope => slopeAngle > 0.1f && slopeAngle <= maxSlopeAngle;

        /// <summary>True if slope is too steep to stand on</summary>
        public bool IsOnSteepSlope => slopeAngle > maxSlopeAngle;

        /// <summary>Time since player was last grounded</summary>
        public float TimeSinceGrounded => Time.time - lastGroundedTime;

        // ============ INITIALIZATION ============

        private void Awake()
        {
            if (feetPosition == null)
            {
                feetPosition = transform;
                Debug.LogWarning("[GroundChecker] No feet position assigned, using transform.");
            }
        }

        // ============ UPDATE ============

        private void FixedUpdate()
        {
            wasGroundedLastFrame = isGrounded;
            CheckGround();

            if (isGrounded)
            {
                lastGroundedTime = Time.time;
            }
        }

        // ============ GROUND CHECK LOGIC ============

        private void CheckGround()
        {
            Vector2 origin = feetPosition.position;

            // Cast three rays: center, left, right
            centerHit = Physics2D.Raycast(origin, Vector2.down, raycastDistance, groundLayer);
            leftHit = Physics2D.Raycast(origin + Vector2.left * sideRayOffset, Vector2.down, raycastDistance, groundLayer);
            rightHit = Physics2D.Raycast(origin + Vector2.right * sideRayOffset, Vector2.down, raycastDistance, groundLayer);

            // Grounded if ANY raycast hits
            isGrounded = centerHit.collider != null || leftHit.collider != null || rightHit.collider != null;

            // Calculate ground normal (prioritize center hit)
            if (centerHit.collider != null)
            {
                groundNormal = centerHit.normal;
            }
            else if (leftHit.collider != null)
            {
                groundNormal = leftHit.normal;
            }
            else if (rightHit.collider != null)
            {
                groundNormal = rightHit.normal;
            }
            else
            {
                groundNormal = Vector2.up;
            }

            // Calculate slope angle
            slopeAngle = Vector2.Angle(groundNormal, Vector2.up);
        }

        // ============ PUBLIC METHODS ============

        /// <summary>
        /// Returns the perpendicular direction for movement on slopes.
        /// Use this to move along slopes without sliding.
        /// </summary>
        public Vector2 GetSlopeDirection(float horizontalInput)
        {
            if (!IsOnSlope || horizontalInput == 0)
            {
                return new Vector2(horizontalInput, 0);
            }

            // Calculate perpendicular to ground normal
            Vector2 slopeDir = Vector2.Perpendicular(groundNormal);
            
            // Ensure direction matches input
            if ((horizontalInput > 0 && slopeDir.x < 0) || (horizontalInput < 0 && slopeDir.x > 0))
            {
                slopeDir = -slopeDir;
            }

            return slopeDir.normalized;
        }

        /// <summary>
        /// Updates raycast parameters from PlayerMovementData.
        /// Call this if movement data changes at runtime.
        /// </summary>
        public void UpdateParameters(float distance, float width, float maxAngle)
        {
            raycastDistance = distance;
            sideRayOffset = width;
            maxSlopeAngle = maxAngle;
        }

        /// <summary>
        /// Checks if a specific point is over ground.
        /// Useful for predictive checks.
        /// </summary>
        public bool IsPointOverGround(Vector2 point, float distance)
        {
            return Physics2D.Raycast(point, Vector2.down, distance, groundLayer).collider != null;
        }

        /// <summary>
        /// Returns distance to ground from feet position.
        /// Returns float.MaxValue if no ground detected.
        /// </summary>
        public float GetDistanceToGround()
        {
            RaycastHit2D hit = Physics2D.Raycast(feetPosition.position, Vector2.down, 100f, groundLayer);
            return hit.collider != null ? hit.distance : float.MaxValue;
        }

        // ============ DEBUG VISUALIZATION ============

        private void OnDrawGizmos()
        {
            if (!showDebugGizmos) return;

            Vector2 origin = feetPosition != null ? (Vector2)feetPosition.position : (Vector2)transform.position;
            
            Gizmos.color = isGrounded ? groundedColor : airborneColor;

            // Draw center ray
            Gizmos.DrawLine(origin, origin + Vector2.down * raycastDistance);
            
            // Draw left ray
            Vector2 leftOrigin = origin + Vector2.left * sideRayOffset;
            Gizmos.DrawLine(leftOrigin, leftOrigin + Vector2.down * raycastDistance);
            
            // Draw right ray
            Vector2 rightOrigin = origin + Vector2.right * sideRayOffset;
            Gizmos.DrawLine(rightOrigin, rightOrigin + Vector2.down * raycastDistance);

            // Draw ground normal if grounded
            if (isGrounded)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(origin, origin + groundNormal * 0.5f);
            }
        }
    }
}
