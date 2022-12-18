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
        private float _currentValue;
        private float _maxValue;
        private List<StatModifier> _modifiers;
        private List<ModifiersID> _acceptedModifiers;
        private StatID _id;
        
        public float CurrentValue => _currentValue;
        public float MaxValue => _maxValue + _modifiers.Sum(modifier => modifier.Value);

        ~SingleStat()
        {
            _modifiers.Clear();
            _acceptedModifiers.Clear();
        }
        
        public void ConfigureStat(StatID newStatID, float maxValue, IEnumerable<ModifiersID> acceptedModifiers)
        {
            _id = newStatID;
            _maxValue = maxValue;
            _currentValue = _maxValue;

            _modifiers = new List<StatModifier>();
            _acceptedModifiers = new List<ModifiersID>(acceptedModifiers);
        }

        public void AddModifier(StatModifier modifier, bool addToCurrentValue = false)
        {
            if(!CanAddModifier(modifier))
                return;
            
            _modifiers.Add(modifier);
            
            if(!addToCurrentValue)
                return;
            
            AddToValue(modifier.Value);
        }

        public void RemoveModifier(StatModifier modifier)
        {
            if(!_modifiers.Contains(modifier))
                return;

            _modifiers.Remove(modifier);
        }

        public void AddToValue(float valueToAdd)
        {
            _currentValue += valueToAdd;
            _currentValue = _currentValue.ClampToMax(MaxValue);
        }

        public void UpdateValue(float newValue)
        {
            _currentValue = newValue;
            _currentValue = _currentValue.ClampToMax(MaxValue);
        }

        private bool CanAddModifier(StatModifier modifier) =>
            _acceptedModifiers != null && _acceptedModifiers.Contains(modifier.ID);
    }
}
