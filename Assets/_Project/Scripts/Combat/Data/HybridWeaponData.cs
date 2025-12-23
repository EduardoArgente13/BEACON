using UnityEngine;
using BEACON.Combat.Weapons;

namespace BEACON.Combat
{
    [CreateAssetMenu(fileName = "New Hybrid Weapon", menuName = "BEACON/Combat/Hybrid Weapon")]
    public class HybridWeaponData : ScriptableObject
    {
        [Header("Identity")]
        public string weaponName;
        public HybridArchetype archetype;
        public Sprite icon;
        public GameObject weaponPrefab;

        [Header("Primary Attack (X)")]
        public ComboData primaryCombo;
        
        [Header("Special Attack (RB)")]
        public ComboData specialCombo; // Usually 1 hit but can be a combo

        [Header("Synergy")]
        [TextArea(3, 5)]
        public string synergyDescription;
        public bool hasSynergy;

        [Header("Visuals")]
        public Color resonanceTrailColor = Color.white;
    }
}
