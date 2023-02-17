using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using FishNet.Object;
using IT.Interfaces;
using IT.ScriptableObjects;
using UnityEngine;

namespace IT.Gameplay
{
    public class PointCounter : NetworkBehaviour, IPointCounter
    {
        [SerializeField] private GameSettings _gameSettings;
        [SerializeField, Sirenix.OdinInspector.ReadOnly] private int _currentPoints;
        
        public int CurrentPoints => _currentPoints;

        public void GainPoints() => _currentPoints = Math.Clamp(_currentPoints + _gameSettings.PointsGainPerTick, 0, _gameSettings.PointsToWin);
    }
}
