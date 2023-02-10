using System.Collections;
using System.Collections.Generic;
using IT.Interfaces;
using UnityEngine;

namespace IT.Gameplay
{
    public class SpawnPointHolder : MonoBehaviour, ISpawnPoint
    {
        private bool _isOccupied;

        public Vector3 SpawnPoint => transform.position;
        public bool IsOccupied => _isOccupied;
        
        public void Claim()
        {
            if(_isOccupied)
                return;

            _isOccupied = true;
        }

        public void Unclaim() => _isOccupied = false;
    }
}
