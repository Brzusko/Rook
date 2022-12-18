using System.Collections;
using System.Collections.Generic;
using IT.Stats;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace IT.Tests
{
    public class StatsTestRunner
    {
        [Test]
        public void CreationTest()
        {
            var stat = new SingleStat();
            stat.ConfigureStat(StatID.HEALTH, 100, new List<ModifiersID> { ModifiersID.HEALTH });
            Assert.IsNotNull(stat);
        }
        
        [Test]
        public void AddModifierTest()
        {
            var stat = new SingleStat();
            stat.ConfigureStat(StatID.HEALTH, 100, new List<ModifiersID> { ModifiersID.HEALTH });
            Assert.IsNotNull(stat);
            
            var healthModifier = new StatModifier
            {
                ID = ModifiersID.HEALTH,
                Value = 100
            };

            var staminaModifier = new StatModifier
            {
                ID = ModifiersID.MOVEMENT_SPEED,
                Value = 100,
            };

            Assert.IsTrue(stat.AddModifier(healthModifier));
            Assert.IsFalse(stat.AddModifier(staminaModifier));
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        
        // [UnityTest]
        // public IEnumerator StatsTestRunnerWithEnumeratorPasses()
        // {
        //     // Use the Assert class to test conditions.
        //     // Use yield to skip a frame.
        //     yield return null;
        // }
    }
}
