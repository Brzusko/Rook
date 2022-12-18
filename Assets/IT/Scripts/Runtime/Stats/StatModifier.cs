using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Stats
{
    public struct StatModifier: IEquatable<StatModifier>
    {
        private static float _equatableTolerance = 0.02f;
        public float Value;
        public ModifiersID ID;

        public bool Equals(StatModifier other)
        {
            return Math.Abs(Value - other.Value) < _equatableTolerance && ID == other.ID;
        }
    }
}
