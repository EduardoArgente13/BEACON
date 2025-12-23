using UnityEngine;
using BEACON.Core;

namespace BEACON.Managers
{
    /// <summary>
    /// Handles game feel effects: screen shake, camera effects, visual feedback.
    /// Uses trauma-based screen shake for natural intensity decay.
    /// </summary>
    public class GameFeedback : Singleton<GameFeedback>
    {
        // ============ CONFIGURATION ============

        [Header("Screen Shake Settings")]
        [SerializeField, Tooltip("Maximum shake offset in units")]
        private float maxShakeOffset = 0.5f;

        [SerializeField, Tooltip("Maximum rotation in degrees")]
        private float maxShakeRotation = 3f;

        [SerializeField, Tooltip("Shake noise frequency")]
        private float shakeFrequency = 25f;

        [SerializeField, Tooltip("How fast trauma decays (per second)")]
        private float traumaDecay = 1.5f;

        [SerializeField, Tooltip("Power curve for trauma (2 = quadratic, more intense at high trauma)")]
        private float traumaPower = 2f;

        [Header("References")]
        [SerializeField, Tooltip("Camera to shake (auto-finds main camera if null)")]
        private Camera targetCamera;

        // ============ STATE ============

        private float currentTrauma;
        private Vector3 originalCameraPosition;
        private float seed;

        // ============ PROPERTIES ============

        /// <summary>Current trauma level (0-1)</summary>
        public float Trauma => currentTrauma;

        /// <summary>Computed shake intensity based on trauma</summary>
        public float ShakeIntensity => Mathf.Pow(currentTrauma, traumaPower);

        // ============ INITIALIZATION ============

        protected override void OnSingletonAwake()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            if (targetCamera != null)
            {
                originalCameraPosition = targetCamera.transform.localPosition;
            }

            seed = Random.value * 1000f;
        }

        // ============ UPDATE ============

        private void Update()
        {
            // Decay trauma over time
            if (currentTrauma > 0)
            {
                currentTrauma = Mathf.Max(0, currentTrauma - traumaDecay * Time.unscaledDeltaTime);
                ApplyShake();
            }
            else if (targetCamera != null && targetCamera.transform.localPosition != originalCameraPosition)
            {
                // Reset camera position when shake ends
                targetCamera.transform.localPosition = originalCameraPosition;
            }
        }

        // ============ PUBLIC METHODS ============

        /// <summary>
        /// Adds trauma to trigger screen shake.
        /// Trauma is clamped to 1, but you can overshoot for guaranteed strong shake.
        /// </summary>
        /// <param name="amount">Trauma amount (0-1 typical, can exceed 1)</param>
        public void AddTrauma(float amount)
        {
            currentTrauma = Mathf.Clamp01(currentTrauma + amount);
        }

        /// <summary>
        /// Convenience method for common shake intensities.
        /// </summary>
        public void ShakeLight() => AddTrauma(0.2f);
        public void ShakeMedium() => AddTrauma(0.4f);
        public void ShakeHeavy() => AddTrauma(0.7f);
        public void ShakeExtreme() => AddTrauma(1f);

        /// <summary>
        /// Instantly sets trauma level.
        /// </summary>
        public void SetTrauma(float amount)
        {
            currentTrauma = Mathf.Clamp01(amount);
        }

        /// <summary>
        /// Stops all shake immediately.
        /// </summary>
        public void StopShake()
        {
            currentTrauma = 0;
            if (targetCamera != null)
            {
                targetCamera.transform.localPosition = originalCameraPosition;
            }
        }

        /// <summary>
        /// Updates the original camera position (call after camera moves).
        /// </summary>
        public void UpdateOriginalPosition(Vector3 position)
        {
            originalCameraPosition = position;
        }

        // ============ SHAKE CALCULATION ============

        private void ApplyShake()
        {
            if (targetCamera == null) return;

            float intensity = ShakeIntensity;
            float time = Time.unscaledTime * shakeFrequency;

            // Use Perlin noise for smooth, natural shake
            float offsetX = (Mathf.PerlinNoise(seed, time) * 2 - 1) * maxShakeOffset * intensity;
            float offsetY = (Mathf.PerlinNoise(seed + 100, time) * 2 - 1) * maxShakeOffset * intensity;
            float rotation = (Mathf.PerlinNoise(seed + 200, time) * 2 - 1) * maxShakeRotation * intensity;

            // Apply offset
            targetCamera.transform.localPosition = originalCameraPosition + new Vector3(offsetX, offsetY, 0);
            
            // Apply rotation on Z axis
            targetCamera.transform.localRotation = Quaternion.Euler(0, 0, rotation);
        }

        // ============ FREEZE FRAME ============

        /// <summary>
        /// Triggers a hitstop (freeze frame) for impact feedback.
        /// Delegates to TimeManager.
        /// </summary>
        /// <param name="duration">Freeze duration in real-time seconds</param>
        public void DoHitstop(float duration)
        {
            TimeManager.Instance?.DoHitstop(duration);
        }

        /// <summary>
        /// Triggers both hitstop and screen shake for powerful impacts.
        /// </summary>
        /// <param name="freezeDuration">Hitstop duration</param>
        /// <param name="shakeTrauma">Shake trauma amount</param>
        public void DoImpact(float freezeDuration, float shakeTrauma)
        {
            DoHitstop(freezeDuration);
            AddTrauma(shakeTrauma);
        }

        // ============ PRESETS ============

        /// <summary>Light hit - small shake, tiny freeze</summary>
        public void ImpactLight()
        {
            DoImpact(0.02f, 0.15f);
        }

        /// <summary>Medium hit - noticeable shake and freeze</summary>
        public void ImpactMedium()
        {
            DoImpact(0.04f, 0.35f);
        }

        /// <summary>Heavy hit - strong shake and freeze</summary>
        public void ImpactHeavy()
        {
            DoImpact(0.08f, 0.6f);
        }

        /// <summary>Critical/Boss hit - dramatic shake and freeze</summary>
        public void ImpactCritical()
        {
            DoImpact(0.12f, 0.9f);
        }

        /// <summary>Parry success - distinct feedback</summary>
        public void ImpactParry()
        {
            DoImpact(0.06f, 0.4f);
        }

        // ============ DEBUG ============

        #if UNITY_EDITOR
        private void OnGUI()
        {
            if (currentTrauma > 0)
            {
                GUILayout.BeginArea(new Rect(Screen.width - 160, 10, 150, 60));
                GUILayout.Label($"Trauma: {currentTrauma:F2}");
                GUILayout.Label($"Intensity: {ShakeIntensity:F2}");
                GUILayout.EndArea();
            }
        }
        #endif
    }
}
