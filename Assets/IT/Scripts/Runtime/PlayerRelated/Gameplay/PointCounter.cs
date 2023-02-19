using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using IT.Interfaces;
using IT.ScriptableObjects;
using IT.ScriptableObjects.UI;
using UnityEngine;

namespace IT.Gameplay
{
    public class PointCounter : NetworkBehaviour, IPointCounter
    {
        [SerializeField] private GameSettings _gameSettings;
        [SerializeField] private GameplayBinding _gameplayBinding;
        [SerializeField, Sirenix.OdinInspector.ReadOnly, SyncVar(OnChange = nameof(OnCurrentPointsChange))] private int _currentPoints;
        
        public int CurrentPoints => _currentPoints;

        public void GainPoints() => _currentPoints = Math.Clamp(_currentPoints + _gameSettings.PointsGainPerTick, 0, _gameSettings.PointsToWin);

        private void OnCurrentPointsChange(int prev, int next, bool asServer)
        {
            if(asServer)
                return;
            
            _gameplayBinding.FireUpdatePoints(NetworkObject.ObjectId, next);
        }
    }
}
