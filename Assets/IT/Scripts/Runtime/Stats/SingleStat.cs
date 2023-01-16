using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using IT.Utils.PrimitiveExtensions;

namespace IT.Stats
{
    [System.Serializable]
    public class SingleStat
    {
        private StatID _id;
        private float _currentValue;
        private float _maxValue;
        private float _modifiersValue = 1f;
        private List<StatModifier> _modifierList = new List<StatModifier>();

        public float CurrentValue
        {
            get => _currentValue;
            set
            {
                _currentValue = value;
                _currentValue = _currentValue.ClampToMax(MaxValue);
            }
        }

        public float MaxValue
        {
            get => _maxValue * _modifiersValue;
            set
            {
                _maxValue = value;

                if (_maxValue < 0f)
                    _maxValue = 0f;
            }
        }

        public float Modifier => _modifiersValue;

        public SingleStat(StatID id, float maxValue, IEnumerable<StatModifier> modifiers = null)
        {
            _id = id;

            if (modifiers != null)
                _modifierList = modifiers.ToList();
            
            CalculateModifiersValue();
            _maxValue = maxValue;
            _currentValue = MaxValue;
        }

        ~SingleStat()
        {
            _modifierList.Clear();
        }

        public bool AddModifier(StatModifier modifier, bool updateCurrentValue = false)
        {
            if(modifier.ID != _id)
                return false;
            
            _modifierList.Add(modifier);
            var cacheMaxValue = MaxValue;
            CalculateModifiersValue();
            
            if(!updateCurrentValue)
                return true;

            var deltaMaxValue = MaxValue - cacheMaxValue;
            CurrentValue += deltaMaxValue;
            return true;
        }

        public bool RemoveModifier(StatModifier modifier, bool updateCurrentValue = false)
        {
            if (_modifierList == null || modifier.ID != _id)
                return false;

            var result = _modifierList.Remove(modifier);

            if (!result)
                return false;
            
            var maxValueCache = MaxValue;
            CalculateModifiersValue();
            
            if (_currentValue > MaxValue)
            {
                _currentValue = MaxValue;
                return true;
            }

            if (!updateCurrentValue)
                return true;

            var deltaMaxValue = MaxValue - maxValueCache;
            CurrentValue += deltaMaxValue;
            
            return true;
        }
        
        private void CalculateModifiersValue()
        {
            if(_modifierList == null)
                return;

            float multiplicationResult = 1.0f;

            foreach (StatModifier modifier in _modifierList)
                multiplicationResult *= modifier.Value;

            _modifiersValue = multiplicationResult;
        }
    }
}
