using UnityEngine;

namespace BEACON.Player
{
    /// <summary>
    /// Detects walls for wall slide and wall jump mechanics.
    /// Uses horizontal raycasts to detect walls and their direction.
    /// </summary>
    public class WallChecker : MonoBehaviour
    {
        // ============ CONFIGURATION ============

        [Header("Wall Detection")]
        [SerializeField, Tooltip("Layer(s) considered as walls")]
        private LayerMask wallLayer;

        [SerializeField, Tooltip("Transform for upper wall raycast")]
        private Transform upperCheckPoint;

        [SerializeField, Tooltip("Transform for lower wall raycast")]
        private Transform lowerCheckPoint;

        [SerializeField, Tooltip("Length of wall check raycasts")]
        private float raycastDistance = 0.2f;

        [Header("Debug")]
        [SerializeField]
        private bool showDebugGizmos = true;

        [SerializeField]
        private Color touchingWallColor = Color.yellow;

        [SerializeField]
        private Color notTouchingColor = Color.gray;

        // ============ STATE ============

        private bool isTouchingWallLeft;
        private bool isTouchingWallRight;
        private RaycastHit2D leftUpperHit;
        private RaycastHit2D leftLowerHit;
        private RaycastHit2D rightUpperHit;
        private RaycastHit2D rightLowerHit;

        // ============ PUBLIC PROPERTIES ============

        /// <summary>True if touching any wall</summary>
        public bool IsTouchingWall => isTouchingWallLeft || isTouchingWallRight;

        /// <summary>True if touching wall on left side</summary>
        public bool IsTouchingWallLeft => isTouchingWallLeft;

        /// <summary>True if touching wall on right side</summary>
        public bool IsTouchingWallRight => isTouchingWallRight;

        /// <summary>Returns -1 for left wall, 1 for right wall, 0 for no wall</summary>
        public int WallDirection
        {
            get
            {
                if (isTouchingWallRight) return 1;
                if (isTouchingWallLeft) return -1;
                return 0;
            }
        }

        /// <summary>Returns the wall normal of the currently touched wall</summary>
        public Vector2 WallNormal
        {
            get
            {
                if (isTouchingWallRight)
                {
                    return rightUpperHit.collider != null ? rightUpperHit.normal : Vector2.left;
                }
                if (isTouchingWallLeft)
                {
                    return leftUpperHit.collider != null ? leftUpperHit.normal : Vector2.right;
                }
                return Vector2.zero;
            }
        }

        // ============ INITIALIZATION ============

        private void Awake()
        {
            ValidateCheckPoints();
        }

        private void ValidateCheckPoints()
        {
            if (upperCheckPoint == null || lowerCheckPoint == null)
            {
                Debug.LogWarning("[WallChecker] Check points not assigned. Creating default positions.");
                
                if (upperCheckPoint == null)
                {
                    var upper = new GameObject("WallCheck_Upper");
                    upper.transform.SetParent(transform);
                    upper.transform.localPosition = new Vector3(0, 0.5f, 0);
                    upperCheckPoint = upper.transform;
                }

                if (lowerCheckPoint == null)
                {
                    var lower = new GameObject("WallCheck_Lower");
                    lower.transform.SetParent(transform);
                    lower.transform.localPosition = new Vector3(0, -0.3f, 0);
                    lowerCheckPoint = lower.transform;
                }
            }
        }

        // ============ UPDATE ============

        private void FixedUpdate()
        {
            CheckWalls();
        }

        // ============ WALL CHECK LOGIC ============

        private void CheckWalls()
        {
            // Check left side (both upper and lower must hit for valid wall)
            leftUpperHit = Physics2D.Raycast(upperCheckPoint.position, Vector2.left, raycastDistance, wallLayer);
            leftLowerHit = Physics2D.Raycast(lowerCheckPoint.position, Vector2.left, raycastDistance, wallLayer);
            isTouchingWallLeft = leftUpperHit.collider != null && leftLowerHit.collider != null;

            // Check right side
            rightUpperHit = Physics2D.Raycast(upperCheckPoint.position, Vector2.right, raycastDistance, wallLayer);
            rightLowerHit = Physics2D.Raycast(lowerCheckPoint.position, Vector2.right, raycastDistance, wallLayer);
            isTouchingWallRight = rightUpperHit.collider != null && rightLowerHit.collider != null;
        }

        // ============ PUBLIC METHODS ============

        /// <summary>
        /// Checks if touching wall in a specific direction.
        /// </summary>
        /// <param name="facingRight">True to check right side, false for left</param>
        public bool IsTouchingWallInDirection(bool facingRight)
        {
            return facingRight ? isTouchingWallRight : isTouchingWallLeft;
        }

        /// <summary>
        /// Returns the opposite direction of the wall for wall jump.
        /// </summary>
        public Vector2 GetWallJumpDirection()
        {
            if (isTouchingWallRight) return Vector2.left;
            if (isTouchingWallLeft) return Vector2.right;
            return Vector2.zero;
        }

        /// <summary>
        /// Updates raycast distance parameter.
        /// </summary>
        public void UpdateParameters(float distance)
        {
            raycastDistance = distance;
        }

        // ============ DEBUG VISUALIZATION ============

        private void OnDrawGizmos()
        {
            if (!showDebugGizmos) return;
            if (upperCheckPoint == null || lowerCheckPoint == null) return;

            // Left side
            Gizmos.color = isTouchingWallLeft ? touchingWallColor : notTouchingColor;
            Gizmos.DrawLine(upperCheckPoint.position, (Vector2)upperCheckPoint.position + Vector2.left * raycastDistance);
            Gizmos.DrawLine(lowerCheckPoint.position, (Vector2)lowerCheckPoint.position + Vector2.left * raycastDistance);

            // Right side
            Gizmos.color = isTouchingWallRight ? touchingWallColor : notTouchingColor;
            Gizmos.DrawLine(upperCheckPoint.position, (Vector2)upperCheckPoint.position + Vector2.right * raycastDistance);
            Gizmos.DrawLine(lowerCheckPoint.position, (Vector2)lowerCheckPoint.position + Vector2.right * raycastDistance);
        }
    }
}
