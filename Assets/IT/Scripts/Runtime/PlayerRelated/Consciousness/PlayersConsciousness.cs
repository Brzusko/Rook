using System;
using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing.Server;
using FishNet.Object;
using FishNet.Transporting;
using IT.Interfaces;
using Sirenix.Utilities;
using UnityEngine;

namespace IT
{
    public class PlayersConsciousness : NetworkBehaviour, IPlayersConsciousness, IConsciousnessCreator
    {
        [SerializeField] private NetworkObject _playerConsciousnessReference;

        private ServerManager _serverManager;
        private bool _areEventsBound;

        private SortedList<NetworkConnection, IPlayerConsciousness> _playerConsciousnesses;

        private void Initialize()
        {
            _serverManager = InstanceFinder.ServerManager;
            int maxClients = ServiceContainer.Get<INetworkBridge>().MaxClients;
            _playerConsciousnesses = new SortedList<NetworkConnection, IPlayerConsciousness>(maxClients);
        }
        
        private void BindEvents()
        {
            if(_areEventsBound)
                return;
            
            _serverManager.OnRemoteConnectionState += OnRemoteConnectionState;
            _areEventsBound = true;
        }

        private void UnbindEvents()
        {
            if(!_areEventsBound)
                return;

            _serverManager.OnRemoteConnectionState -= OnRemoteConnectionState;
            _areEventsBound = false;
        }

        private void OnRemoteConnectionState(NetworkConnection connection, RemoteConnectionStateArgs args)
        {
            if(args.ConnectionState != RemoteConnectionState.Stopped)
                return;
            
            ClearConsciousnessForConnection(connection);
        }

        private void ClearConsciousnessForConnection(NetworkConnection connection)
        {
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

        #region Interfaces

        public IPlayerConsciousness CreateConsciousness(NetworkConnection connection)
        {
            if (_playerConsciousnesses.ContainsKey(connection))
                return null;

            NetworkObject playerConsciousnessInstance = Instantiate(_playerConsciousnessReference);
            playerConsciousnessInstance.name = $"{nameof(IPlayerConsciousness)}[{connection.ClientId.ToString()}]";
            IPlayerConsciousness playerConsciousness = playerConsciousnessInstance.GetComponent<IPlayerConsciousness>();

            if (playerConsciousness != null)
            {
                _playerConsciousnesses.Add(connection, playerConsciousness);
                return playerConsciousness;
            }

            Debug.LogError($"{nameof(playerConsciousness)} is null, provide correct prefab");
            Destroy(playerConsciousnessInstance);
            
            return null;
        }
        
        public void PossessAll()
        {
            if(_playerConsciousnesses == null)
                return;

            _playerConsciousnesses.Values.ForEach(playerConsciousness => playerConsciousness.Possess());
        }

        public void UnpossessAll()
        {
            if(_playerConsciousnesses == null)
                return;
            
            _playerConsciousnesses.Values.ForEach(playerConsciousness => playerConsciousness.Unpossess());
        }
        
        public void Clear()
        {
            if(_playerConsciousnesses == null || _playerConsciousnesses.Count == 0)
                return;
            
            _playerConsciousnesses.Values.ForEach(pred => pred.Clear());
            _playerConsciousnesses.Clear();
        }
        
        #endregion
        
    }
}
