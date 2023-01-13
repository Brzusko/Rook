using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Managing.Server;
using FishNet.Object;
using FishNet.Transporting;
using IT.Interfaces;
using UnityEngine;

namespace IT
{
    public class PlayersConsciousness : NetworkBehaviour, IPlayersConsciousness
    {
        [SerializeField] private NetworkObject _playerConsciousnessReference;

        private ServerManager _serverManager;
        private Dictionary<NetworkConnection, IPlayerConsciousness> _playerConsciousnesses;
        private bool _areEventsBound;

        private void Initialize()
        {
            _playerConsciousnesses = new Dictionary<NetworkConnection, IPlayerConsciousness>();
        }

        private void Clear()
        {
            _playerConsciousnesses.Clear();
            _playerConsciousnesses = null;
        }

        private void BindEvents()
        {
            if(_areEventsBound)
                return;

            _areEventsBound = true;
        }

        private void UnbindEvents()
        {
            if(!_areEventsBound)
                return;

            _serverManager.OnRemoteConnectionState += OnRemoteConnectionState;
            _areEventsBound = false;
        }

        private void OnRemoteConnectionState(NetworkConnection connection, RemoteConnectionStateArgs args)
        {
            if(args.ConnectionState != RemoteConnectionState.Stopped)
                return;
            
            if(!_playerConsciousnesses.ContainsKey(connection))
                return;
            
            //clear consciousness
            _playerConsciousnesses.Remove(connection);
        }
        
        public override void OnStartServer()
        {
            base.OnStartServer();
            
            Initialize();
            BindEvents();
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            
            Clear();
            UnbindEvents();
        }

        public IPlayerConsciousness CreatePlayerConsciousness(NetworkConnection connection)
        {
            if (_playerConsciousnesses.ContainsKey(connection))
            {
                Debug.LogError("Could not create consciousness!");
                return null;
            }

            NetworkObject networkObject = Instantiate(_playerConsciousnessReference);
            IPlayerConsciousness playerConsciousness = networkObject.GetComponent<IPlayerConsciousness>();

            if (playerConsciousness == null)
            {
                Debug.LogError($"Provided network object is not implementing {nameof(IPlayerConsciousness)}");
                Destroy(networkObject);
                return null;
            }
            
            return playerConsciousness;
        }
        
        
    }
}
