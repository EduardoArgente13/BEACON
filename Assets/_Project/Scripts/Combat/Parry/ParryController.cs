using UnityEngine;
using System.Collections;
using BEACON.Combat.Core;
using BEACON.Combat.Feedback;

namespace BEACON.Combat.Parry
{
    public class ParryController : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private CombatController combatController;
        [SerializeField] private BEACON.Core.Stats.ResonanceController resonanceController;
        [SerializeField] private ParryData parryData;

        [Header("Detection")]
        [SerializeField] private Transform parryCenter;
        [SerializeField] private float parryRadius = 1.5f;
        [SerializeField] private LayerMask parriableLayers;

        public event System.Action<ParryResult> OnParryPerformed;
        public UnityEngine.Events.UnityEvent OnPerfectParry;

        private bool isParryActive;
        private float parryWindowStart;
        private Coroutine parryCoroutine;

        private void Awake()
        {
            if (combatController == null) combatController = GetComponent<CombatController>();
            if (resonanceController == null) resonanceController = GetComponent<BEACON.Core.Stats.ResonanceController>();
        }

        public void ActivateParry()
        {
            if (parryData == null)
            {
                Debug.LogError("[Parry] Missing Parry Data!");
                return;
            }

            if (parryCoroutine != null) StopCoroutine(parryCoroutine);
            parryCoroutine = StartCoroutine(ParryRoutine());
        }

        private IEnumerator ParryRoutine()
        {
            isParryActive = true;
            parryWindowStart = Time.time;
            
            // Visual Debug trigger
            Debug.Log("[Parry] Window Open");

            // Check for attacks continuously or just once? 
            // Usually, active frames are short. Let's check for a few frames.
            // For this implementation, we'll check continuously during the Good Window.
            
            float elapsedTime = 0f;
            bool success = false;

            while (elapsedTime < parryData.goodWindow)
            {
                success = CheckForParry();
                if (success) break;

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            if (!success)
            {
                // Failed parry (Recovery / Vulnerability)
                Debug.Log("[Parry] Failed (Window timeout)");
                // Trigger punishment if needed
                HandleParryResult(ParryResult.Failed, null);
            }

            isParryActive = false;
        }

        private bool CheckForParry()
        {
            // Simple overlap check for demonstration. 
            // In a pro system, enemies might "register" attacks to the player.
            // Here we look for active Hitboxes labeled as EnemyAttack.
            
            Collider2D[] hits = Physics2D.OverlapCircleAll(parryCenter.position, parryRadius, parriableLayers);

            foreach (var hit in hits)
            {
                var parriable = hit.GetComponent<IParriable>();
                if (parriable != null && parriable.CanBeParried())
                {
                    EvaluateParry(parriable);
                    return true;
                }
            }
            return false;
        }

        private void EvaluateParry(IParriable target)
        {
            float timeSinceStart = Time.time - parryWindowStart;
            ParryResult result = new ParryResult();
            
            if (timeSinceStart <= parryData.perfectWindow)
            {
                result.timing = ParryTiming.Perfect;
                result.resonanceGained = parryData.perfectResonanceGain;
                result.enemyStunDuration = parryData.perfectStunDuration;
                Debug.Log($"<color=cyan>[Parry] PERFECT! ({timeSinceStart:F3}s)</color>");
                
                if (resonanceController != null) resonanceController.AddResonance(parryData.perfectResonanceGain);
                OnPerfectParry?.Invoke();

                // Feedback
                if (HitStopManager.Instance) HitStopManager.Instance.TriggerHitStop(parryData.perfectHitStop);
                if (CombatCameraShake.Instance) CombatCameraShake.Instance.AddTrauma(0.6f);
            }
            else
            {
                result.timing = ParryTiming.Good;
                result.resonanceGained = parryData.goodResonanceGain;
                result.enemyStunDuration = parryData.goodStunDuration;
                Debug.Log($"<color=green>[Parry] Good ({timeSinceStart:F3}s)</color>");

                if (resonanceController != null) resonanceController.AddResonance(parryData.goodResonanceGain);

                if (HitStopManager.Instance) HitStopManager.Instance.TriggerHitStop(parryData.goodHitStop);
                if (CombatCameraShake.Instance) CombatCameraShake.Instance.AddTrauma(0.3f);
            }

            result.successfulReference = true;
            target.OnParried(result);
            HandleParryResult(result, target);
        }

        private void HandleParryResult(ParryResult result, IParriable target)
        {
            OnParryPerformed?.Invoke(result);
            
            if (result.timing == ParryTiming.Failed)
            {
                // Optional: Force state to stunned or vulnerable
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (parryCenter != null)
            {
                Gizmos.color = isParryActive ? Color.cyan : Color.gray;
                Gizmos.DrawWireSphere(parryCenter.position, parryRadius);
            }
        }
    }
}
