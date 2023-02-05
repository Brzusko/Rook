using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using FishNet.Object;
using IT.Interfaces;
using UnityEngine;

namespace IT.Gameplay
{
    public class PointCounter : NetworkBehaviour, IPointCounter
    {
        [SerializeField] private uint _pointGainAmmount;
        [SerializeField, Sirenix.OdinInspector.ReadOnly] private uint _currentPoints;
        
        public uint CurrentPoints => _currentPoints;
        
        public void GainPoints() => _currentPoints += _pointGainAmmount;
        public void ResetCounter() => _currentPoints = 0;
    }
}
