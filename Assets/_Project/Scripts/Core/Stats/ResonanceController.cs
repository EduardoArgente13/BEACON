using UnityEngine;
using UnityEngine.Events;

namespace BEACON.Core.Stats
{
    public class ResonanceController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float maxResonance = 100f;
        [SerializeField] private float decayRate = 2f; // Slower decay (was 5)
        [SerializeField] private float decayDelay = 5.0f; // Longer grace period (was 2)
        [SerializeField] private float safeDecayThreshold = 0f; // Won't decay below this amount

        [Header("Events")]
        public UnityEvent<float, float> OnResonanceChanged; // current, max
        public UnityEvent OnFullResonance;

        public float CurrentResonance { get; private set; }
        public float MaxResonance => maxResonance;

        private float lastChangeTime;

        private void Start()
        {
            CurrentResonance = 0f;
            UpdateUI();
        }

        private void Update()
        {
            // Only decay if above threshold and delay passed
            if (CurrentResonance > safeDecayThreshold && Time.time >= lastChangeTime + decayDelay)
            {
                float decayAmount = decayRate * Time.deltaTime;
                // Ensure we don't decay below threshold
                float newAmount = Mathf.Max(CurrentResonance - decayAmount, safeDecayThreshold);
                // We use direct set or SpendResonance but SpendResonance clamps to 0.
                // Let's manually set or modify Spend to support target.
                // Simplest: Calculate delta
                float toRemove = CurrentResonance - newAmount;
                if (toRemove > 0)
                {
                    SpendResonance(toRemove);
                }
            }
        }

        public void AddResonance(float amount)
        {
            CurrentResonance = Mathf.Clamp(CurrentResonance + amount, 0f, maxResonance);
            lastChangeTime = Time.time;
            UpdateUI();

            if (CurrentResonance >= maxResonance)
            {
                OnFullResonance?.Invoke();
                Debug.Log("[Resonance] MAX REACHED! Void State Ready.");
            }
        }

        public bool TrySpendResonance(float amount)
        {
            if (CurrentResonance >= amount - 0.01f)
            {
                SpendResonance(amount);
                lastChangeTime = Time.time; 
                return true;
            }
            Debug.LogWarning($"[Resonance] Failed spend: Current({CurrentResonance}) < Needed({amount})");
            return false;
        }

        private void SpendResonance(float amount)
        {
            CurrentResonance = Mathf.Clamp(CurrentResonance - amount, 0f, maxResonance);
            UpdateUI();
        }

        private void UpdateUI()
        {
            OnResonanceChanged?.Invoke(CurrentResonance, maxResonance);
        }
    }
}
