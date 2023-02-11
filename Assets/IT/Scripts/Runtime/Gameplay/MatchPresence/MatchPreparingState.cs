using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Transporting;
using IT.Interfaces;
using IT.Interfaces.FSM;
using UnityEngine;

namespace IT.Gameplay
{
    public class MatchPreparingState : NetworkBehaviour, IState<MatchStatesID>
    {
        [SerializeField] private GameObject _stateMachineGameObject;
        [SerializeField] private GameObject _playersConsciousnessGameObject;
        
        private IStateMachine<MatchStatesID> _stateMachine;
        private IPlayersConsciousness _playersConsciousness;

        private bool _areEventsBound;
        
        public MatchStatesID StateID => MatchStatesID.PREPARING;

        private void Awake()
        {
            InitializeOnce();
        }

        private void OnDestroy()
        {
            UnbindEvents();
        }

        private void InitializeOnce()
        {
            _stateMachine = _stateMachineGameObject.GetComponent<IStateMachine<MatchStatesID>>();
            _playersConsciousness = _playersConsciousnessGameObject.GetComponent<IPlayersConsciousness>();
        }

        private void BindEvents()
        {
            if(_areEventsBound)
                return;
            
            ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
            
            _areEventsBound = true;
        }

        private void UnbindEvents()
        {
            if(!_areEventsBound)
                return;
            
            ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;
            _areEventsBound = false;
        }
        
        private void OnRemoteConnectionState(NetworkConnection connection, RemoteConnectionStateArgs connectionStateArgs)
        {
            if(connectionStateArgs.ConnectionState != RemoteConnectionState.Stopped)
                return;
            
            Debug.Log(ServerManager.Clients.Count);
            
            if(ServerManager.Clients.Count > 1)
                return;
            
            _stateMachine.ChangeState(MatchStatesID.WAITING, true);
        }

        public void Enter(bool asServer)
        {
            if (asServer)
            {
                _playersConsciousness.PossessAll();
                BindEvents();
            }
        }

        public void Exit(bool asServer)
        {
            if (asServer)
            {
                UnbindEvents();   
            }
        }
    }
}
