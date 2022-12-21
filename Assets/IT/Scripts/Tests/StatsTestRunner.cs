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
        private static float _maxFloatTolerance = 0.01f;
        private static float _maxValueStartingPoint = 10f;
        
        [Test]
        public void CreateSingleStat()
        {
            var stat = new SingleStat(StatID.HEALTH, _maxValueStartingPoint, null);
            Assert.IsTrue((Math.Abs(stat.CurrentValue - _maxValueStartingPoint) < _maxFloatTolerance) && Math.Abs(stat.MaxValue - _maxValueStartingPoint) < _maxFloatTolerance);
            stat.CurrentValue = 300f;
            Assert.IsTrue(Math.Abs(stat.CurrentValue - _maxValueStartingPoint) < _maxValueStartingPoint);
        }

        [Test]
        public void ModifierChanging()
        {
            var stat = new SingleStat(StatID.HEALTH, _maxValueStartingPoint,
                new List<StatModifier> { new StatModifier { ID = StatID.HEALTH, Value = 2.0f } });
            
            Assert.IsTrue(stat.CurrentValue - (_maxValueStartingPoint * 2f) < _maxFloatTolerance);
            Assert.IsTrue(stat.RemoveModifier(new StatModifier{ ID = StatID.HEALTH, Value = 2.0f }));
            Assert.IsTrue(Math.Abs(stat.CurrentValue - _maxValueStartingPoint) < _maxFloatTolerance);
            Assert.IsTrue(stat.AddModifier(new StatModifier { ID = StatID.HEALTH, Value = 1.1f}));
            Assert.IsFalse(stat.AddModifier(new StatModifier {ID = StatID.STAMINA, Value = 10f}));
            Assert.IsTrue(stat.AddModifier(new StatModifier{ID = StatID.HEALTH, Value = 0.1f}, true));
            Assert.IsTrue(Math.Abs(stat.CurrentValue - (_maxValueStartingPoint + 1)) < _maxFloatTolerance);
            Assert.IsTrue(stat.RemoveModifier(new StatModifier { ID = StatID.HEALTH, Value = 1.1f}, true));
            Assert.IsTrue(stat.RemoveModifier(new StatModifier{ID = StatID.HEALTH, Value = 0.1f}, true));
            Assert.IsTrue(Math.Abs(stat.CurrentValue - _maxValueStartingPoint) < _maxFloatTolerance);
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
