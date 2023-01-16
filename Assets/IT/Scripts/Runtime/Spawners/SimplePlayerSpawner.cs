using System;
using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Managing.Server;
using FishNet.Object;
using IT.Interfaces;
using UnityEngine;
using Random = UnityEngine.Random;

namespace IT.Spawners
{
    public class SimplePlayerSpawner : NetworkBehaviour
    {
        [SerializeField] private NetworkObject _playerPrefab;
        [SerializeField] private List<Transform> _spawnLocations;
        [SerializeField] private GameObject _playersConsciousnessGameObject;
        [SerializeField] private GameObject _playerFactoryGameObject;

        [SerializeField] private NetworkObject _playerEntityPrefab;
            
        private bool _areEventsBound;
        private SceneManager _sceneManager;
        private ServerManager _serverManager;
        private IConsciousnessCreator _playersConsciousnessCreator;
        private IPlayerFactory _playerFactory;

        private void Start()
        {
            InitializeOnce();
        }

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
            _sceneManager.OnClientLoadedStartScenes += OnClientPresenceChangeEnd;
        }

        private void UnbindEvents()
        {
            if (!_areEventsBound)
                return;

            _areEventsBound = false;
            _sceneManager.OnClientLoadedStartScenes -= OnClientPresenceChangeEnd;
        }

        private void InitializeOnce()
        {
            _sceneManager = InstanceFinder.SceneManager;
            _serverManager = InstanceFinder.ServerManager;
            _playersConsciousnessCreator = _playersConsciousnessGameObject.GetComponent<IConsciousnessCreator>();
            //_playerFactory = _playerFactoryGameObject.GetComponent<IPlayerFactory>();
        }

        private void OnClientPresenceChangeEnd(NetworkConnection conn, bool asServer)
        {
            SpawnPlayer(conn);
        }

        private void SpawnPlayer(NetworkConnection conn)
        {
            IPlayerConsciousness playerConsciousness = _playersConsciousnessCreator.CreateConsciousness(conn);

            if (playerConsciousness == null)
            {
                Debug.LogError("Could not spawn player consciousness");
                return;
            }

            int spawnIndex = Random.Range(0, _spawnLocations.Count - 1);
            Vector3 spawnLocation = _spawnLocations[spawnIndex].position;
            NetworkObject playerEntityInstance = Instantiate(_playerEntityPrefab, spawnLocation, Quaternion.identity);
            IEntityToPossess playerEntity = playerEntityInstance.GetComponent<IEntityToPossess>();
            
            _serverManager.Spawn(playerConsciousness.NetworkObject, conn);
            _serverManager.Spawn(playerEntityInstance, conn);
            
            playerConsciousness.Possess(playerEntity);
        }
    }
}
