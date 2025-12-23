using NUnit.Framework;
using UnityEngine;
using BEACON.Player.Data;

namespace BEACON.Tests.EditMode
{
    public class MovementDataTests
    {
        [Test]
        public void Acceleration_Switches_By_Grounded_State()
        {
            var data = ScriptableObject.CreateInstance<PlayerMovementData>();
            data.groundAcceleration = 50f;
            data.airAcceleration = 25f;

            Assert.AreEqual(50f, data.GetAcceleration(true));
            Assert.AreEqual(25f, data.GetAcceleration(false));
        }

        [Test]
        public void ApexModifiers_Returns_Bonus_Near_Zero_Velocity()
        {
            var data = ScriptableObject.CreateInstance<PlayerMovementData>();
            data.apexThreshold = 2.5f;
            data.apexSpeedBonus = 0.5f;
            data.apexGravityReduction = 0.4f;

            var (speedBonus, gravityMult) = data.GetApexModifiers(0.5f);

            Assert.Greater(speedBonus, 0f);
            Assert.Less(gravityMult, 1f);
        }
    }
}
