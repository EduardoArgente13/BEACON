using UnityEngine;

namespace BEACON.Combat
{
    // ============ CORE STATES ============
    
    public enum CombatState
    {
        Idle,           // Can initiate any action, moving freely
        Attacking,      // Currently executing an attack
        Recovery,       // Brief cooldown/end-lag after attack
        Parrying,       // Active parry window
        Stunned,        // Taking damage/stunned
        VoidState,      // Special powered-up state
        Ability,        // Using a special ability
        Clashing        // Locked in a weapon clash
    }

    // ============ HITBOXES ============

    public enum HitboxShape
    {
        Box,        // Standard rectangle (swords, thrusts)
        Circle,     // Radial (explosions, spins)
        Capsule,    // Arcs (swings)
        Custom      // Polygon2D (complex shapes)
    }

    // ============ COMBO SYSTEM ============

    public enum ComboTiming
    {
        Early,      // Before optimal window
        Perfect,    // Inside optimal window
        Late,       // Ending window (penalty)
        Missed      // Dropped combo
    }

    // ============ MECHANICS ============

    public enum ParryResult
    {
        Perfect,    // Frame-perfect
        Good,       // Within window
        Failed,     // Missed timing
        None        // No parry active
    }

    public enum ClashOutcome
    {
        PlayerWin,
        EnemyWin,
        Stalemate
    }

    public enum ResonanceGainType
    {
        NormalHit,
        ComboHit,
        PerfectTiming,
        Parry,
        PerfectParry,
        ClashWin,
        AerialHit,
        Finisher
    }

    // ============ DATA ============
    
    public enum WeaponType
    {
        Sword,      // Balanced
        Spear,      // Range/Thrust
        Hammer,     // Heavy/Stun
        DualBlades, // Fast/DPS
        Scythe      // Aerial/Wide
    }

    public enum AttackType
    {
        Melee,          // Standard hit, 0 cost
        SuperMelee,     // Heavy hit, spends Resonance
        Ranged,         // Light shot, spends low Resonance
        SuperRanged,    // Heavy shot, spends high Resonance
        Utility         // Buffs/Dashes (Optional)
    }

    public enum HybridArchetype
    {
        Melee_SuperMelee,
        Melee_Ranged,
        Melee_SuperRanged,
        Ranged_SuperRanged,
        Ranged_Melee,
        Ranged_SuperMelee
    }
}
