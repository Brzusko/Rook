using System;
using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing.Server;
using FishNet.Object;
using IT.Interfaces;
using IT.Lobby;
using UnityEngine;

namespace IT.Gameplay
{
    public class MatchPlayerSpawner : MonoBehaviour, IPlayerSpawner<LobbyWaiter>
    {
        [SerializeField] private NetworkObject _playerPrefab;
        [SerializeField] private GameObject[] _spawnPointsGameObject;
        [SerializeField] private GameObject _playerConsciousnessGameObject;

        private IConsciousnessCreator _consciousnessCreator;
        private IPlayersConsciousness _playersConsciousness;
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

            _consciousnessCreator = _playerConsciousnessGameObject.GetComponent<IConsciousnessCreator>();
            _playersConsciousness = _playerConsciousnessGameObject.GetComponent<IPlayersConsciousness>();
        }

        private Vector3 GetFreeSpawnPoint()
        {
            Vector3 point = default;

            foreach (ISpawnPoint spawnPoint in _spawnPoints)
            {
                if(spawnPoint.IsOccupied)
                    continue;
                
                spawnPoint.Claim();
                point = spawnPoint.SpawnPoint;
            }

            return point;
        }
        
        public void SpawnPlayers(IEnumerable<LobbyWaiter> waiters)
        {
            foreach(LobbyWaiter waiter in waiters)
            {
                IPlayerConsciousness playerConsciousness = _consciousnessCreator.CreateConsciousness(waiter.Connection);
                NetworkObject playerInstance = Instantiate(_playerPrefab, GetFreeSpawnPoint(), Quaternion.identity);
                playerInstance.name = $"Player: {waiter.Connection.ClientId.ToString()}";
                IEntityToPossess entityToPossess = playerInstance.GetComponent<IEntityToPossess>();

                if (entityToPossess == null)
                {
                    Destroy(playerConsciousness.NetworkObject.gameObject);
                    Destroy(playerInstance);
                    
                    continue;
                }
                
                playerConsciousness.BindEntity(entityToPossess);
                
                InstanceFinder.ServerManager.Spawn(playerConsciousness.NetworkObject, waiter.Connection);
                InstanceFinder.ServerManager.Spawn(playerInstance);
            }
        }

        public void UnclaimSpawnpoints()
        {
            _spawnPoints.ForEach(spawnPoint => spawnPoint.Unclaim());
        }
    }
}
