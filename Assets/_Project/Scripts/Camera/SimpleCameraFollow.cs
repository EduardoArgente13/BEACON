using UnityEngine;

namespace BEACON.Cameras
{
    /// <summary>
    /// Simple smooth camera follow.
    /// Place this on a PARENT object (CameraRig), and put the MainCamera as a child.
    /// The MainCamera child can then handle shaking locally.
    /// </summary>
    public class SimpleCameraFollow : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0, 1, -10);
        
        [Header("Settings")]
        [SerializeField] private float smoothSpeed = 0.125f;

        private void LateUpdate()
        {
            if (target == null)
            {
                // Try to find player automatically if connection lost
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) 
                {
                    target = player.transform;
                    Debug.Log("[CameraFollow] Player found and assigned as target.");
                }
                else
                {
                    // Prevent spamming log every frame, maybe just once? leaving it simple for now or checking frame count
                    if (Time.frameCount % 60 == 0) Debug.LogWarning("[CameraFollow] Waiting for Player... (Tag your player as 'Player'!)");
                }
                return;
            }

            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;
        }
    }
}
