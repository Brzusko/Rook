using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using IT.Interfaces;
using UnityEngine;

namespace IT.Gameplay
{
    public class MatchPlayerSpawner : MonoBehaviour, IPlayerSpawner
    {
        [SerializeField] private NetworkObject _playerPrefab;
        [SerializeField] private GameObject[] _spawnPointsGameObject;
        [SerializeField] private GameObject _playerConsciousnessGameObject;

        private List<ISpawnPoint> _spawnPoints;

        private void Awake()
        {
            InitializeOnce();
        }

        private void InitializeOnce()
        {
            _spawnPoints = new List<ISpawnPoint>();

            foreach (GameObject spawnPointGameObject in _spawnPointsGameObject)
            {
                if(!spawnPointGameObject.TryGetComponent(out ISpawnPoint spawnPoint))
                    continue;
                
                if(_spawnPoints.Contains(spawnPoint))
                    continue;
                
                _spawnPoints.Add(spawnPoint);
            }
        }

        public void SpawnPlayers(IEnumerable<NetworkConnection> connections)
        {
            throw new System.NotImplementedException();
        }

        public void UnclaimSpawnpoints()
        {
            _spawnPoints.ForEach(spawnPoint => spawnPoint.Unclaim());
        }
    }
}
