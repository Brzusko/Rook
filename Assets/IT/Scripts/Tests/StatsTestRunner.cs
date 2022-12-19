using System;
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
        public void CreateSingleStat()
        {
            var singleStat = new SingleStat(StatID.HEALTH, 100f, 1f);
            Assert.IsTrue(singleStat.Modifier == 1f);
            Assert.IsTrue(singleStat.CurrentValue == 100f);
            Assert.IsTrue(singleStat.MaxValue == 100f);
        }

        [Test]
        public void UpdateCurrentValue()
        {
            var singleStat = new SingleStat(StatID.HEALTH, 100f, 1f);
            singleStat.CurrentValue = -10f;
            Assert.IsTrue(singleStat.CurrentValue == -10f);
            singleStat.CurrentValue = 300f;
            Assert.IsTrue(singleStat.CurrentValue == 100f);
        }

        [Test]
        public void UpdateMaxValue()
        {
            var singleStat = new SingleStat(StatID.HEALTH, 100f, 1f);
            singleStat.UpdateMaxValue(200);
            Assert.IsTrue(singleStat.MaxValue == 200f);
            singleStat.UpdateMaxValue(300f, true);
            Assert.IsTrue(singleStat.CurrentValue == 200f);
            singleStat.UpdateMaxValue(200f, true);
            Assert.IsTrue(singleStat.CurrentValue == 100f);
        }

        [Test]
        public void UpdateModifierValue()
        {
            var singleStat = new SingleStat(StatID.HEALTH, 10f, 1f);
            singleStat.UpdateModifier(1.1f, true);
            Assert.IsTrue(singleStat.CurrentValue == 11.0f);
            singleStat.UpdateModifier(1.0f, true);
            Assert.IsTrue(singleStat.CurrentValue == 10.0f);
            singleStat.UpdateModifier(1.2f);
            Assert.IsTrue(singleStat.MaxValue == 12.0f);
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
