using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet.Connection;
using FishNet.Managing.Server;
using FishNet.Object;
using FishNet.Transporting;
using IT.Interfaces;
using Sirenix.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace IT.Lobby
{
    public class Lobby : NetworkBehaviour, ILobby<LobbyWaiter>
    {
        public event Action<int, int> WaitersStateChange;

        private Dictionary<NetworkConnection, LobbyWaiter> _readyConnectionsDictionary;
        private bool _areEventsBound;

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

            ServerManager.GetAuthenticator().OnAuthenticationResult += OnAuthenticationResult;
            ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
            _areEventsBound = true;
        }
        
        private void UnbindEvents()
        {
            if(!_areEventsBound)
                return;
            
            ServerManager.GetAuthenticator().OnAuthenticationResult -= OnAuthenticationResult;
            ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;
            _areEventsBound = false;
        }

        #region Networking

        [ServerRpc(RequireOwnership = false)]
        private void Server_ChangeWaiterState(NetworkConnection caller, bool newState)
        {
            if (!_readyConnectionsDictionary.ContainsKey(caller))
            {
                caller.Kick(KickReason.ExploitAttempt);
                return;
            }

            _readyConnectionsDictionary[caller].IsReady = newState;

            int readyWaiters = _readyConnectionsDictionary.Values.Count(pred => pred.IsReady);
            
            WaitersStateChange?.Invoke(_readyConnectionsDictionary.Count, readyWaiters);
            
            SendLobbyData();
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void Server_RequestLobbyData()
        {
            SendLobbyData();
        }
        
        [ObserversRpc]
        private void Observers_ReceiveLobbyData(LobbyWaiterSendData[] waiterSendData)
        {
            // propagate to UI
        }

        #endregion
        
        private void OnAuthenticationResult(NetworkConnection connection, bool result)
        {
            if (!result)
            {
                connection.Kick(KickReason.Unset);
                return;
            }
            
            if(_readyConnectionsDictionary.ContainsKey(connection))
                return;
            
            _readyConnectionsDictionary.Add(connection, CreateWaiter(connection));
            SendLobbyData();
        }
        
        private void OnRemoteConnectionState(NetworkConnection connection, RemoteConnectionStateArgs connectionStateArgs)
        {
            if(connectionStateArgs.ConnectionState != RemoteConnectionState.Stopped)
                return;
            
            if(!_readyConnectionsDictionary.ContainsKey(connection))
                return;

            _readyConnectionsDictionary.Remove(connection);
            SendLobbyData();
        }

        private void SendLobbyData()
        {
            List<LobbyWaiterSendData> lobbyWaiters = new List<LobbyWaiterSendData>();
            
            _readyConnectionsDictionary.Values.ForEach(lobbyWaiter =>
            {
                lobbyWaiters.Add(lobbyWaiter.ToSendData());
            });
            
            Observers_ReceiveLobbyData(lobbyWaiters.ToArray());
        }

        private LobbyWaiter CreateWaiter(NetworkConnection conn)
        {
            return new LobbyWaiter
            {
                Connection = conn,
                IsReady = false,
                WaiterColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f)
            };
        }

        public void OpenLobby()
        {
            INetworkBridge networkBridge = ServiceContainer.Get<INetworkBridge>();

            networkBridge.ShouldAcceptConnections = true;
            networkBridge.MaxClients = 4;

            _readyConnectionsDictionary = new Dictionary<NetworkConnection, LobbyWaiter>();
    
            foreach (NetworkConnection conn in ServerManager.Clients.Values)
            {
                _readyConnectionsDictionary.Add(conn, CreateWaiter(conn));
            }
            
            BindEvents();
        }

        public void CloseLobby()
        {
            INetworkBridge networkBridge = ServiceContainer.Get<INetworkBridge>();

            networkBridge.ShouldAcceptConnections = false;
            networkBridge.MaxClients = ServerManager.Clients.Count;

            _readyConnectionsDictionary.Clear();
            
            UnbindEvents();
        }

        public IEnumerable<LobbyWaiter> FetchWaiters()
        {
            return _readyConnectionsDictionary.Values;
        }
        
        [Client]
        public void RequestData()
        {
            Server_RequestLobbyData();
        }
    }
}
