using UnityEngine;
using BEACON.Combat;

namespace BEACON.Combat.Weapons
{
    [CreateAssetMenu(fileName = "New Combo", menuName = "BEACON/Combat/Combo Data")]
    public class ComboData : ScriptableObject
    {
        [Header("Identity")]
        public string comboName;
        
        [Header("Sequence")]
        [Tooltip("Ordered list of attacks in this combo")]
        public AttackData[] attacks;
        
        [Header("Timing")]
        [Tooltip("Time windows (in seconds) to input the next attack for each step")]
        public float[] timingWindows; 
        
        [Header("Properties")]
        public bool allowDirectionalInput;
        public bool resetOnAir;
    }
}
