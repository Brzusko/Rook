using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet.Connection;
using FishNet.Managing.Server;
using FishNet.Object;
using FishNet.Transporting;
using IT.Interfaces;
using IT.ScriptableObjects.UI;
using Sirenix.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace IT.Lobby
{
    public class Lobby : NetworkBehaviour, ILobby<LobbyWaiter>
    {
        public event Action<int, int> WaitersStateChange;

        [SerializeField] private LobbyBinding _lobbyBinding;
        
        private Dictionary<NetworkConnection, LobbyWaiter> _readyConnectionsDictionary;
        private bool _areServerEventsBound;
        private bool _areClientEventsBound;
        private bool _isOpen;

        private bool _isReady;

        #region Initialization

        public override void OnStartClient()
        {
            base.OnStartClient();
            
            BindClientEvents();
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
            
            UnbindClientEvents();
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            UnbindServerEvents();
            
            _readyConnectionsDictionary.Clear();
        }
 
        private void BindServerEvents()
        {
            if(_areServerEventsBound)
                return;

            SceneManager.OnClientLoadedStartScenes += OnClientLoadedStartScenes;
            ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
            _areServerEventsBound = true;
        }
        

        private void UnbindServerEvents()
        {
            if(!_areServerEventsBound)
                return;
            
            SceneManager.OnClientLoadedStartScenes -= OnClientLoadedStartScenes;
            ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;
            _areServerEventsBound = false;
        }

        private void BindClientEvents()
        {
            if(_areClientEventsBound)
                return;


            _lobbyBinding.ReadyClicked += OnReadyClicked;
            _lobbyBinding.ExitClicked += OnExitClicked;
            
            _areClientEventsBound = true;
        }

        private void UnbindClientEvents()
        {
            if(!_areClientEventsBound)
                return;


            _lobbyBinding.ReadyClicked -= OnReadyClicked;
            _lobbyBinding.ExitClicked -= OnExitClicked;
            
            _areClientEventsBound = false;
        }

        #endregion

        #region Networking

        [ServerRpc(RequireOwnership = false)]
        private void Server_ChangeWaiterState(bool newState, NetworkConnection caller = null)
        {
            if (!_readyConnectionsDictionary.ContainsKey(caller) || !_isOpen)
            {
                caller.Kick(KickReason.ExploitAttempt);
                return;
            }

            _readyConnectionsDictionary[caller].IsReady = newState;

            InvokeWaitersStateChangeEvent();
            
            SendLobbyData();
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void Server_RequestLobbyData(NetworkConnection caller = null)
        {
            if (!_readyConnectionsDictionary.ContainsKey(caller) || !_isOpen)
            {
                caller.Kick(KickReason.ExploitAttempt);
                return;
            }
            
            SendLobbyData();
        }
        
        [ObserversRpc]
        private void Observers_ReceiveLobbyData(LobbyWaiterSendData[] waiterSendData)
        {
            _lobbyBinding.FireLobbyWaitersPropagation(waiterSendData);
        }

        #endregion

        #region Events
        
        private void OnClientLoadedStartScenes(NetworkConnection connection, bool asServer)
        {
            if(!asServer)
                return;
            
            if (!connection.Authenticated)
            {
                connection.Kick(KickReason.Unset);
                return;
            }
            
            if(_readyConnectionsDictionary.ContainsKey(connection))
                return;
            
            _readyConnectionsDictionary.Add(connection, CreateWaiter(connection));
            InvokeWaitersStateChangeEvent();
            
            SendLobbyData();
        }

        private void OnRemoteConnectionState(NetworkConnection connection, RemoteConnectionStateArgs connectionStateArgs)
        {
            if(connectionStateArgs.ConnectionState != RemoteConnectionState.Stopped)
                return;
            
            if(!_readyConnectionsDictionary.ContainsKey(connection))
                return;

            _readyConnectionsDictionary.Remove(connection);
            InvokeWaitersStateChangeEvent();
            
            SendLobbyData();
        }

        private void OnReadyClicked()
        {
            _isReady = !_isReady;
            Server_ChangeWaiterState(_isReady);
        }

        private void OnExitClicked()
        {
            INetworkBridge networkBridge = ServiceContainer.Get<INetworkBridge>();

            if (IsHost)
            {
                networkBridge.StopServer();
                return;
            }
            
            networkBridge.StopClient();
        }

        #endregion

        #region Private

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

        private void InvokeWaitersStateChangeEvent()
        {
            int readyWaiters = _readyConnectionsDictionary.Values.Count(pred => pred.IsReady);
            WaitersStateChange?.Invoke(_readyConnectionsDictionary.Count, readyWaiters);
        }

        #endregion

        #region Interface

        public void OpenLobby()
        {
            if (_isOpen)
                return;

            INetworkBridge networkBridge = ServiceContainer.Get<INetworkBridge>();

            networkBridge.ShouldAcceptConnections = true;
            networkBridge.MaxClients = 4;

            _readyConnectionsDictionary = new Dictionary<NetworkConnection, LobbyWaiter>();
    
            foreach (NetworkConnection conn in ServerManager.Clients.Values)
            {
                if(conn.Disconnecting)
                    continue;
                
                _readyConnectionsDictionary.Add(conn, CreateWaiter(conn));
            }

            _isOpen = true;
            BindServerEvents();
        }

        public void CloseLobby()
        {
            if(!_isOpen)
                return;
            
            INetworkBridge networkBridge = ServiceContainer.Get<INetworkBridge>();

            networkBridge.ShouldAcceptConnections = false;
            networkBridge.MaxClients = ServerManager.Clients.Count;

            _readyConnectionsDictionary.Clear();

            _isOpen = false;
            UnbindServerEvents();
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

        #endregion

    }
}
