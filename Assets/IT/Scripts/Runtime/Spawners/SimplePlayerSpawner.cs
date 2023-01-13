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
            
        private bool _areEventsBound;
        private SceneManager _sceneManager;
        private ServerManager _serverManager;
        private IPlayersConsciousness _playersConsciousness;
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
            _playersConsciousness = _playersConsciousnessGameObject.GetComponent<IPlayersConsciousness>();
            //_playerFactory = _playerFactoryGameObject.GetComponent<IPlayerFactory>();
        }

        private void OnClientPresenceChangeEnd(NetworkConnection conn, bool asServer)
        {
            SpawnPlayer(conn);
        }

        private void SpawnPlayer(NetworkConnection conn)
        {
            IPlayerConsciousness playerConsciousness = _playersConsciousness.CreatePlayerConsciousness(conn);

            if (playerConsciousness == null)
            {
                Debug.LogError("Could not spawn player consciousness");
                return;
            }

            playerConsciousness.NetworkObject.name =
                $"{playerConsciousness.NetworkObject.name}[{conn.ClientId.ToString()}]";
            
            _serverManager.Spawn(playerConsciousness.NetworkObject, conn);
        }
    }
}
