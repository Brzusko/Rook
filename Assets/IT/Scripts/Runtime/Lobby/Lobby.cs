using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet.Connection;
using FishNet.Managing.Server;
using FishNet.Object;
using FishNet.Transporting;
using UnityEngine;

namespace IT.Lobby
{
    public class Lobby : NetworkBehaviour
    {
        public event Action<int, int> WaitersStateChange; 
        
        private Dictionary<NetworkConnection, bool> _readyConnectionsDictionary;
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
            
            _readyConnectionsDictionary.Clear();
        }

        private void BindEvents()
        {
            if(_areEventsBound)
                return;

            ServerManager.OnRemoteConnectionState += OnRemoteConnectionStateChange;
            SceneManager.OnClientLoadedStartScenes += OnClientLoadedStartScenes;
            
            _areEventsBound = true;
        }

        private void UnbindEvents()
        {
            if(!_areEventsBound)
                return;
            
            ServerManager.OnRemoteConnectionState -= OnRemoteConnectionStateChange;
            SceneManager.OnClientLoadedStartScenes -= OnClientLoadedStartScenes;
            
            _areEventsBound = false;
        }

        [ServerRpc(RequireOwnership = false)]
        private void Server_ChangeWaiterState(NetworkConnection caller, bool newState)
        {
            if (!_readyConnectionsDictionary.ContainsKey(caller))
            {
                caller.Kick(KickReason.ExploitAttempt);
                return;
            }

            _readyConnectionsDictionary[caller] = newState;

            int readyWaiters = _readyConnectionsDictionary.Values.Count(pred => pred);
            
            WaitersStateChange?.Invoke(_readyConnectionsDictionary.Count, readyWaiters);
        }

        private void OnRemoteConnectionStateChange(NetworkConnection connection, RemoteConnectionStateArgs stateArgs)
        {
            if(stateArgs.ConnectionState != RemoteConnectionState.Stopped)
                return;
            
            if(!_readyConnectionsDictionary.ContainsKey(connection))
                return;

            _readyConnectionsDictionary.Remove(connection);
        }
        
        private void OnClientLoadedStartScenes(NetworkConnection connection, bool asServer)
        {
            if(!asServer)
                return;

            if (_readyConnectionsDictionary.ContainsKey(connection))
            {
                connection.Kick(KickReason.ExploitAttempt);
                return;
            }

            _readyConnectionsDictionary[connection] = false;
        }
    }
}
