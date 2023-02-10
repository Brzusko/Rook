using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using IT.Interfaces;
using IT.Interfaces.FSM;
using IT.Lobby;
using UnityEngine;

namespace IT.Gameplay
{
    public class MatchWaitingState : NetworkBehaviour, IState<MatchStatesID>
    {
        [SerializeField] private GameObject _stateMachineGameObject;
        [SerializeField] private GameObject _lobbyGameObject;
        
        private IStateMachine<MatchStatesID> _stateMachine;
        private ILobby<LobbyWaiter> _lobby;

        private bool _areEventsBound;
        
        public MatchStatesID StateID => MatchStatesID.WAITING;

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
            _lobby = _lobbyGameObject.GetComponent<ILobby<LobbyWaiter>>();
            _stateMachine = _stateMachineGameObject.GetComponent<IStateMachine<MatchStatesID>>();
        }

        private void BindEvents()
        {
            if(_areEventsBound)
                return;

            _lobby.WaitersStateChange += OnWaiterReady;
            _areEventsBound = true;
        }

        private void UnbindEvents()
        {
            if(!_areEventsBound)
                return;
            
            _lobby.WaitersStateChange -= OnWaiterReady;
            _areEventsBound = false;
        }

        private void OnWaiterReady(int totalCount, int readyCount)
        {
            if(totalCount != readyCount)
                return;
            
            _stateMachine.ChangeState(MatchStatesID.PREPARING, true);
        }
        
        public void Enter(bool asServer)
        {
            if (asServer)
            {
                _lobby.OpenLobby();
                BindEvents();
                return;
            }
        }

        public void Exit(bool asServer)
        {
            if (asServer)
            {
                //fetch waiters connection
                //close lobby
                //unbind events
                //spawn players
                return;
            }
        }
    }
}
