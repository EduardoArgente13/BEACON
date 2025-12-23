using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using BEACON.Core.Stats;

namespace BEACON.Tests.PlayMode
{
    public class ResonanceControllerPlayModeTests
    {
        [UnityTest]
        public IEnumerator AddResonance_Clamps_To_Max()
        {
            var go = new GameObject("ResonanceTest");
            var controller = go.AddComponent<ResonanceController>();

            SetPrivateField(controller, "maxResonance", 50f);
            SetPrivateField(controller, "decayRate", 0f);
            SetPrivateField(controller, "decayDelay", 999f);

            // Let Unity call Start()
            yield return null;

            controller.AddResonance(100f);
            Assert.AreEqual(50f, controller.CurrentResonance, 0.01f);
        }

        [UnityTest]
        public IEnumerator TrySpendResonance_Fails_When_Insufficient()
        {
            var go = new GameObject("ResonanceSpendTest");
            var controller = go.AddComponent<ResonanceController>();

            SetPrivateField(controller, "maxResonance", 30f);
            SetPrivateField(controller, "decayRate", 0f);
            SetPrivateField(controller, "decayDelay", 999f);

            yield return null;

            controller.AddResonance(10f);
            bool spent = controller.TrySpendResonance(20f);
            Assert.IsFalse(spent, "Should not spend more Resonance than available");
            Assert.AreEqual(10f, controller.CurrentResonance, 0.01f);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Field '{fieldName}' not found on {target.GetType().Name}");
            field.SetValue(target, value);
        }
    }
}
