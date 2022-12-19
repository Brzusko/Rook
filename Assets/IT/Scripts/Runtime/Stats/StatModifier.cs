using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Stats
{
    [System.Serializable]
    public struct StatModifier: IEquatable<StatModifier>
    {
        private static float _equatableTolerance = 0.02f;
        public float Value;
        public StatID ID;

        public bool Equals(StatModifier other)
        {
            return Math.Abs(Value - other.Value) < _equatableTolerance && ID == other.ID;
        }

        public static bool operator ==(StatModifier lhs, StatModifier rhs)
        {
            return lhs.ID == rhs.ID && Math.Abs(lhs.Value - rhs.Value) < _equatableTolerance;
        }

        public static bool operator !=(StatModifier lhs, StatModifier rhs)
        {
            return !(lhs == rhs);
        }
    }
}
