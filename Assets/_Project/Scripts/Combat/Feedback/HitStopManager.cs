using UnityEngine;
using System.Collections;

namespace BEACON.Combat.Feedback
{
    /// <summary>
    /// Manages "HitStop" (Freeze Frame) effects to give weight to impacts.
    /// freezes time briefly when a hit connects.
    /// </summary>
    public class HitStopManager : MonoBehaviour
    {
        private static HitStopManager instance;
        public static HitStopManager Instance => instance;

        private bool isWaiting;

        private void Awake()
        {
            if (instance == null) instance = this;
            else Destroy(gameObject);
        }

        public void TriggerHitStop(float duration)
        {
            if (isWaiting) return;
            if (duration <= 0) return;

            // Debug log to confirm it's working
            Debug.Log($"[HitStop] Freezing time for {duration}s");

            StartCoroutine(DoHitStop(duration));
        }

        private IEnumerator DoHitStop(float duration)
        {
            isWaiting = true;
            float originalScale = Time.timeScale;
            
            Time.timeScale = 0f;
            
            // Use real time to wait while game time is frozen
            yield return new WaitForSecondsRealtime(duration);
            
            Time.timeScale = originalScale;
            isWaiting = false;
        }
    }
}
