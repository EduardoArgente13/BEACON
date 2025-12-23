using UnityEngine;

namespace BEACON.Combat.Parry
{
    public enum ParryInteractionType
    {
        Destructive, // Destroy the object (e.g. Weak Projectile)
        Reflective,  // Reflect the object (e.g. Strong Projectile)
        Stun         // Stun the attacker (e.g. Melee Enemy)
    }

    /// <summary>
    /// Implemented by Enemy Attacks or Projectiles.
    /// Allows the ParryController to evaluate if it can be parried.
    /// </summary>
    public interface IParriable
    {
        bool CanBeParried();
        ParryInteractionType InteractionType { get; }
        float GetDamage();
        GameObject GetSource();
        
        /// <summary>
        /// Called when the player successfully parries this object.
        /// Use this to stun the enemy or deflect the projectile.
        /// </summary>
        void OnParried(ParryResult result);
    }
}
