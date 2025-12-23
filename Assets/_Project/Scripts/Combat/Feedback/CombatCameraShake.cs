using UnityEngine;

namespace BEACON.Combat.Feedback
{
    /// <summary>
    /// Handles procedural camera shake for combat impact.
    /// Needs to be placed on the Camera or a parent holder.
    /// </summary>
    public class CombatCameraShake : MonoBehaviour
    {
        private static CombatCameraShake instance;
        public static CombatCameraShake Instance => instance;

        [Header("Settings")]
        [SerializeField] private float maxShakeDistance = 0.5f;
        [SerializeField] private float shakeFrequency = 10f;
        [SerializeField] private float traumaDecay = 1.5f; // How fast shaking stops

        private float trauma; // 0 to 1
        private Vector3 initialPosition;
        private float seed;

        private void Awake()
        {
            if (instance == null) instance = this;
            initialPosition = transform.localPosition;
            seed = Random.value * 100f;
        }

        private void Update()
        {
            if (trauma > 0)
            {
                // Decrease trauma over time
                trauma = Mathf.Clamp01(trauma - Time.deltaTime * traumaDecay);

                // Shake = Trauma squared (juicier feel)
                float shake = trauma * trauma;

                // Perlin noise for smooth, organic shaking
                float offsetX = maxShakeDistance * shake * (Mathf.PerlinNoise(seed + Time.time * shakeFrequency, seed) - 0.5f) * 2f;
                float offsetY = maxShakeDistance * shake * (Mathf.PerlinNoise(seed, seed + Time.time * shakeFrequency) - 0.5f) * 2f;

                transform.localPosition = initialPosition + new Vector3(offsetX, offsetY, 0);
            }
            else if (transform.localPosition != initialPosition)
            {
                transform.localPosition = initialPosition;
            }
        }

        /// <summary>
        /// Adds trauma to the camera shake system.
        /// Val should be 0-1 (e.g., 0.2 for light hit, 0.5 for heavy).
        /// </summary>
        public void AddTrauma(float amount)
        {
            trauma = Mathf.Clamp01(trauma + amount);
        }
    }
}
