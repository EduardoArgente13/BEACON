using UnityEngine;
using UnityEngine.UI;
using BEACON.Core.Health;
using BEACON.Core.Stats;

namespace BEACON.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Slider healthBar;
        [SerializeField] private Slider resonanceBar;
        
        [Header("Player Components")]
        [SerializeField] private Health playerHealth;
        [SerializeField] private ResonanceController playerResonance;

        private void Start()
        {
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged.AddListener(UpdateHealthUI);
                // Force initial sync
                UpdateHealthUI(playerHealth.CurrentHealth, playerHealth.MaxHealth);
            }

            if (playerResonance != null)
            {
                playerResonance.OnResonanceChanged.AddListener(UpdateResonanceUI);
                // Force initial sync
                UpdateResonanceUI(playerResonance.CurrentResonance, playerResonance.MaxResonance);
            }
        }

        // Ideally, Health script triggers an event: OnHealthChanged(current, max)
        public void UpdateHealthUI(float current, float max)
        {
            if (healthBar != null)
            {
                healthBar.value = current / max;
            }
        }

        public void UpdateResonanceUI(float current, float max)
        {
            if (resonanceBar != null)
            {
                resonanceBar.value = current / max;
            }
        }
    }
}
