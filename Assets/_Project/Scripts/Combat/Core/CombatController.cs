using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BEACON.Player;
using BEACON.Combat;
using BEACON.Combat.Feedback;

namespace BEACON.Combat.Core
{
    /// <summary>
    /// The "Brain" of the combat system. Orchestrates states, inputs, and events.
    /// Integrates with PlayerController for movement locking and input reading.
    /// Supports the Hybrid Weapon system (X = Primary, RB = Special).
    /// </summary>
    public class CombatController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private HitboxController hitboxController;
        [SerializeField] private Transform firePoint; 
        
        [Header("Weapon Configuration")]
        [SerializeField] private HybridWeaponData equippedWeapon;

        // ============ STATE ============
        private PlayerInputHandler inputHandler;
        public CombatState CurrentState { get; private set; } = CombatState.Idle;
        
        private int currentComboIndex;
        private float lastAttackTime;
        private bool canCombo;
        private AttackData? activeAttackData;

        private BEACON.Core.Stats.ResonanceController resonanceController;
        [SerializeField] private BEACON.Combat.Parry.ParryController parryController;

        // ============ INITIALIZATION ============

        private void Awake()
        {
            if (playerController == null) playerController = GetComponent<PlayerController>();
            inputHandler = GetComponent<PlayerInputHandler>(); 
            if (hitboxController == null) hitboxController = GetComponentInChildren<HitboxController>();
            resonanceController = GetComponent<BEACON.Core.Stats.ResonanceController>();
        }

        private void OnEnable()
        {
            if (inputHandler != null)
            {
                inputHandler.OnAttackPressed += TryAttack;
                inputHandler.OnSkillPressed += TrySpecial;
                inputHandler.OnParryPressed += TryParry;
            }
            
            if (hitboxController != null)
            {
                hitboxController.OnHitConfirmed.AddListener(OnHit);
            }
        }

        private void OnDisable()
        {
            if (inputHandler != null)
            {
                inputHandler.OnAttackPressed -= TryAttack;
                inputHandler.OnSkillPressed -= TrySpecial;
                inputHandler.OnParryPressed -= TryParry;
            }
            
            if (hitboxController != null)
            {
                hitboxController.OnHitConfirmed.RemoveListener(OnHit);
            }
        }

        private void Update()
        {
            // Reset combo if idle for too long
            if (CurrentState == CombatState.Idle && currentComboIndex > 0)
            {
                if (Time.time - lastAttackTime > 1.0f) // 1s combo reset tolerance
                {
                    currentComboIndex = 0;
                }
            }
        }

        // ============ ENTRY POINTS ============

        public void TryAttack()
        {
            if (equippedWeapon == null || equippedWeapon.primaryCombo == null) return;
            
            if (CurrentState == CombatState.Idle || (CurrentState == CombatState.Attacking && canCombo))
            {
                StopAllCoroutines();
                StartCoroutine(ExecuteAttackRoutine(equippedWeapon.primaryCombo, true));
            }
        }

        public void TrySpecial()
        {
            if (equippedWeapon == null || equippedWeapon.specialCombo == null) return;

            if (CurrentState == CombatState.Idle || (CurrentState == CombatState.Attacking && canCombo))
            {
                StopAllCoroutines();
                StartCoroutine(ExecuteAttackRoutine(equippedWeapon.specialCombo, false));
            }
        }

        public void TryParry()
        {
            if (CurrentState == CombatState.Idle || CurrentState == CombatState.Recovery)
            {
                if (parryController != null)
                {
                    StopAllCoroutines();
                    CurrentState = CombatState.Parrying;
                    parryController.ActivateParry();
                    StartCoroutine(ParryRecoveryRoutine(0.4f)); 
                }
            }
        }

        private IEnumerator ParryRecoveryRoutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (CurrentState == CombatState.Parrying) CurrentState = CombatState.Idle;
        }

        // ============ CORE REFACTORED ROUTINE ============

        private IEnumerator ExecuteAttackRoutine(BEACON.Combat.Weapons.ComboData combo, bool isPrimary)
        {
            // Determine starting index
            int index = (CurrentState == CombatState.Attacking) ? currentComboIndex : 0;
            if (index >= combo.attacks.Length) index = 0;
            
            AttackData attack = combo.attacks[index];

            // 1. Resource Check
            if (attack.resonanceCost > 0)
            {
                if (resonanceController != null && !resonanceController.TrySpendResonance(attack.resonanceCost))
                {
                    Debug.LogWarning($"[Combat] CANNOT ATTACK: Out of Resonance (Need {attack.resonanceCost}, You have {resonanceController.CurrentResonance})");
                    yield break;
                }
            }

            // 2. State Setup
            CurrentState = CombatState.Attacking;
            currentComboIndex = index;
            canCombo = false;
            activeAttackData = attack;

            // 3. Execution Phase
            int currentFrame = 0;
            bool hitExecuted = false;
            float frameTime = 1f / 60f;

            while (currentFrame < attack.activeFrameEnd + 10) // buffer frames
            {
                // Charge Logic
                if (attack.isCharged && currentFrame == attack.activeFrameStart && !hitExecuted)
                {
                    Debug.Log($"[Combat] Charging {attack.animationName}...");
                    float chargeTimer = 0;
                    bool buttonHeld = isPrimary ? inputHandler.IsAttackHeld : inputHandler.IsSkillHeld;
                    
                    // Wait as long as button is held OR until min charge time is met
                    while (buttonHeld || chargeTimer < attack.minChargeTime)
                    {
                        chargeTimer += Time.deltaTime;
                        buttonHeld = isPrimary ? inputHandler.IsAttackHeld : inputHandler.IsSkillHeld;
                        
                        // Capping charge at 3s for safety
                        if (chargeTimer > 3.0f) break; 
                        yield return null;
                    }
                    Debug.Log($"[Combat] Charge Released at {chargeTimer}s");
                }

                if (currentFrame >= attack.activeFrameStart && currentFrame <= attack.activeFrameEnd && !hitExecuted)
                {
                    ExecuteAttackStep(attack);
                    hitExecuted = true;
                }

                if (currentFrame >= attack.earliestCancelFrame)
                {
                    canCombo = true;
                }

                currentFrame++;
                yield return new WaitForSeconds(frameTime);
            }

            // 4. Cleanup
            lastAttackTime = Time.time;
            
            if (canCombo)
            {
                float window = (index < combo.timingWindows.Length) ? combo.timingWindows[index] : 0.5f;
                yield return new WaitForSeconds(window);
            }

            if (CurrentState == CombatState.Attacking)
            {
                CurrentState = CombatState.Idle;
                currentComboIndex++;
                if (currentComboIndex >= combo.attacks.Length) currentComboIndex = 0;
            }
        }

        private void ExecuteAttackStep(AttackData data)
        {
            switch (data.attackType)
            {
                case AttackType.Melee:
                case AttackType.SuperMelee:
                    ExecuteMeleeHit(data);
                    break;
                case AttackType.Ranged:
                case AttackType.SuperRanged:
                    ExecuteRangedHit(data);
                    break;
            }

            // Standard movement for ANY attack type
            if (data.providesMovement)
            {
                Vector2 dir = playerController.IsFacingRight ? Vector2.right : Vector2.left;
                playerController.ApplyKnockback(dir, data.movementVector.x, data.movementDuration);
                Debug.Log($"[Combat] Applying Attack Movement: {data.movementVector.x} for {data.movementDuration}s");
            }
        }

        private void ExecuteMeleeHit(AttackData data)
        {
            Vector2 knockback = (playerController.IsFacingRight ? Vector2.right : Vector2.left) * data.knockbackForce;
            
            hitboxController.ActivateHitbox(
                data.hitboxShape,
                data.hitboxOffset,
                data.hitboxSize,
                data.damage,
                knockback
            );
        }

        private void ExecuteRangedHit(AttackData data)
        {
            if (data.projectilePrefab == null) return;

            int count = Mathf.Max(1, data.projectileCount);
            Debug.Log($"[Combat] Executing Ranged Attack: {data.projectilePrefab.name} | Count: {count}");

            float startAngle = playerController.IsFacingRight ? 0 : 180;
            Vector2 dir = playerController.IsFacingRight ? Vector2.right : Vector2.left;
            Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + (Vector3)(dir * 1f);

            for (int i = 0; i < count; i++)
            {
                float angleOffset = 0;
                if (count > 1)
                {
                    // Spread math: i=0 is top, i=count-1 is bottom
                    angleOffset = (i * (data.spreadAngle / (count - 1))) - (data.spreadAngle / 2f);
                }

                float finalAngle = startAngle + (playerController.IsFacingRight ? angleOffset : -angleOffset);
                Quaternion rotation = Quaternion.Euler(0, 0, finalAngle);
                Vector2 finalDir = rotation * Vector2.right;

                GameObject proj = Instantiate(data.projectilePrefab, spawnPos, rotation);
                
                var projScript = proj.GetComponent<BEACON.Combat.Projectiles.PlayerProjectile>();
                if (projScript != null)
                {
                    projScript.Initialize(data.damage, finalDir * data.knockbackForce);
                }

                var rb = proj.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = finalDir * data.projectileSpeed;
                }
            }
        }

        // ============ HIT CALLBACK ============

        private void OnHit(GameObject target, Vector2 point, float damage)
        {
            // Visuals
            if (HitStopManager.Instance != null) HitStopManager.Instance.TriggerHitStop(activeAttackData?.hitStopDuration ?? 0.08f);
            if (CombatCameraShake.Instance != null) CombatCameraShake.Instance.AddTrauma(0.2f);

            // Damage
            var damageable = target.GetComponent<BEACON.Core.Interfaces.IDamageable>();
            if (damageable != null)
            {
                Vector2 knockbackDir = (target.transform.position - transform.position).normalized;
                damageable.TakeDamage(damage, knockbackDir * 5f, gameObject);
            }

            // Resonance
            if (resonanceController != null && activeAttackData.HasValue)
            {
                // Any attack can gain resonance if configured, regardless of type
                if (activeAttackData.Value.resonanceGain > 0)
                {
                    resonanceController.AddResonance(activeAttackData.Value.resonanceGain);
                }
            }
        }

        // ============ PUBLIC API ============

        public void EquipWeapon(HybridWeaponData weapon)
        {
            equippedWeapon = weapon;
            currentComboIndex = 0;
            CurrentState = CombatState.Idle;
            Debug.Log($"[Combat] Equipped Hybrid Weapon: {weapon.weaponName}");
        }
    }
}
