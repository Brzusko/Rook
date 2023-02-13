using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet.Connection;
using FishNet.Managing.Server;
using FishNet.Object;
using FishNet.Object.Synchronizing;
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
        public event Action EveryoneReady;

        [SerializeField] private LobbyBinding _lobbyBinding;
        [SerializeField] private float _timerStartValue = 10f;
        
        //Server Vars
        private Dictionary<NetworkConnection, LobbyWaiter> _readyConnectionsDictionary;
        private bool _areServerEventsBound;
        private bool _isOpen;
        
        //Client Vars
        private bool _areClientEventsBound;
        private bool _isReady;
        
        //shared
        private int _timeLeftCache = -1;
        private bool _updateTimer = false;
        
        //Networked Vars
        [SyncObject] private readonly SyncTimer _lobbyTimer = new SyncTimer();

        #region Initialization

        private void Awake()
        {
            _lobbyTimer.OnChange += LobbyTimerOnChange;
        }
        
        private void OnDestroy()
        {
            _lobbyTimer.OnChange -= LobbyTimerOnChange;
        }
        
        private void Update()
        {
            _lobbyTimer.Update(Time.deltaTime); 
           
            if(!IsClient || !_updateTimer)
                return;
           
            if(_lobbyTimer.Paused)
                return;
           
            int timeLeft = (int)_lobbyTimer.Remaining;
           
            if (timeLeft == _timeLeftCache)
            {
                return;
            }
           
            _timeLeftCache = timeLeft;
           
            string message = $"Game starts in {timeLeft.ToString()}.";
           
            _lobbyBinding.FireLobbyMessage(message);
        }

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
            CheckTimer();
            
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

        private void LobbyTimerOnChange(SyncTimerOperation op, float prev, float next, bool asServer)
        {
            if (op == SyncTimerOperation.Start && !asServer)
            {
                _updateTimer = true;
                return;
            }

            if (op == SyncTimerOperation.Finished)
            {
                if (asServer)
                {
                    EveryoneReady?.Invoke();
                    return;
                }
                
                _lobbyBinding.FireLobbyMessage("Waiting for game to start.");
                return;
            }

            if ((op == SyncTimerOperation.Stop || op == SyncTimerOperation.StopUpdated || op == SyncTimerOperation.Pause) && !asServer)
            {
                _lobbyBinding.FireLobbyMessage("Waiting for players to be ready.");
                _updateTimer = false;
            }
        }
        
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

        private void CheckTimer()
        {
            bool areWaitersReady = _readyConnectionsDictionary.Values.All(waiter => waiter.IsReady);

            if (areWaitersReady)
            {
                _lobbyTimer.StartTimer(_timerStartValue);
                return;
            }
            
            _lobbyTimer.StopTimer();
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
