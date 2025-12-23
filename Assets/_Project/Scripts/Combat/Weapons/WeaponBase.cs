using UnityEngine;
using BEACON.Combat;

namespace BEACON.Combat.Weapons
{
    public enum WeaponCategory
    {
        Melee_Generator,
        Ranged_Spender
    }

    [CreateAssetMenu(fileName = "New Weapon", menuName = "BEACON/Combat/Weapon")]
    public class WeaponBase : ScriptableObject
    {
        [Header("Identity")]
        public string weaponName;
        public WeaponCategory category = WeaponCategory.Melee_Generator; // New Category
        public Sprite icon;
        public GameObject weaponPrefab; // Visuals
        
        [Header("Start Stats")]
        public float baseDamage = 10f;
        public float attackSpeed = 1f;
        
        [Header("Combos")]
        public ComboData groundCombo;
        public ComboData airCombo;
        
        [Header("Resonance Economy")]
        [Tooltip("Resonance gained per hit (for Generators)")]
        public float resonanceGain = 5f; 
        
        [Tooltip("Resonance cost per use (for Spenders)")]
        public float resonanceCost = 0f;

        [Header("Ranged Settings")]
        public GameObject projectilePrefab;
        public float projectileSpeed = 15f;
        
        [Header("Features")]
        public bool canParry = true;
        public float parryWindow = 0.2f;
    }
}
