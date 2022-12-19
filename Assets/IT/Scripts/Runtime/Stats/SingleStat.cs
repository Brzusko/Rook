using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using IT.Utils.PrimitiveExtensions;

namespace IT.Stats
{
    [System.Serializable]
    public struct SingleStat
    {
        private float _currentValue;
        private float _maxValue;
        private float _modifier;
        private StatID _id;

        public float CurrentValue
        {
            get => _currentValue;
            set
            {
                _currentValue = value;
                _currentValue = _currentValue.ClampToMax(MaxValue);
            }
        }

        public float Modifier => _modifier;
        public float MaxValue => _maxValue * _modifier;
        public StatID ID => _id;

        public SingleStat(StatID id, float maxValue, float modifier = 1.0f)
        {
            _id = id;
            _maxValue = maxValue;
            _modifier = modifier;
            _currentValue = maxValue * modifier;
        }

        public void UpdateMaxValue(float newMaxValue, bool shouldChangeValue = false)
        {
            var maxValueCache = MaxValue;
            _maxValue = Mathf.Clamp(newMaxValue, 0f, Mathf.Infinity);
            
            if(!shouldChangeValue)
                return;
            
            var maxValueDelta = MaxValue - maxValueCache;
            CurrentValue += maxValueDelta;
        }

        public void UpdateModifier(float newModifierValue, bool shouldChangeValue = false)
        {
            var maxValueCache = MaxValue;
            _modifier = Mathf.Clamp(newModifierValue, 0f, Mathf.Infinity);
            
            if(!shouldChangeValue)
                return;

            var maxValueDelta = MaxValue - maxValueCache;
            CurrentValue += maxValueDelta;
        }
    }
}
