using System;
using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using UnityEngine;
using Random = UnityEngine.Random;

namespace IT.Spawners
{
    public class SimplePlayerSpawner : NetworkBehaviour
    {
        [SerializeField] private NetworkObject _playerPrefab;
        [SerializeField] private List<Transform> _spawnLocations;
        private bool _areEventsBound;

        public override void OnStartServer()
        {
            base.OnStartServer();
            BindEvents();
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            UnbindEvents();
        }
        
        private void BindEvents()
        {
            if (_areEventsBound)
                return;

            _areEventsBound = true;
            InstanceFinder.SceneManager.OnClientLoadedStartScenes += OnClientPresenceChangeEnd;
        }

        private void UnbindEvents()
        {
            if (!_areEventsBound)
                return;

            _areEventsBound = false;
            InstanceFinder.SceneManager.OnClientLoadedStartScenes -= OnClientPresenceChangeEnd;
        }

        private void OnClientPresenceChangeEnd(NetworkConnection conn, bool asServer)
        {
            SpawnPlayer(conn);
        }

        private void SpawnPlayer(NetworkConnection conn)
        {
            int randomIndex = Random.Range(0, _spawnLocations.Count);
            Transform spawnLocation = _spawnLocations[randomIndex];
            NetworkObject playerInstance = Instantiate(_playerPrefab, spawnLocation.position, Quaternion.identity);

            InstanceFinder.ServerManager.Spawn(playerInstance);
        }
    }
}
