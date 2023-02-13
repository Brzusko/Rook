using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using IT.Interfaces;
using IT.Interfaces.FSM;
using IT.UI;
using UnityEngine;
using LobbyWaiter = IT.Lobby.LobbyWaiter;

namespace IT.Gameplay
{
    public class MatchWaitingState : NetworkBehaviour, IState<MatchStatesID>
    {
        [SerializeField] private GameObject _stateMachineGameObject;
        [SerializeField] private GameObject _lobbyGameObject;
        [SerializeField] private GameObject _spawnerGameObject;
        
        private IStateMachine<MatchStatesID> _stateMachine;
        private ILobby<LobbyWaiter> _lobby;
        private IPlayerSpawner<LobbyWaiter> _spawner;

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
            _spawner = _spawnerGameObject.GetComponent<IPlayerSpawner<LobbyWaiter>>();
        }

        private void BindEvents()
        {
            if(_areEventsBound)
                return;

            _lobby.EveryoneReady += OnWaitersReady;
            _areEventsBound = true;
        }

        private void UnbindEvents()
        {
            if(!_areEventsBound)
                return;
            
            _lobby.EveryoneReady -= OnWaitersReady;
            _areEventsBound = false;
        }

        private void OnWaitersReady()
        {
            _stateMachine.ChangeState(MatchStatesID.PREPARING, true);
        }
        
        public void Enter(bool asServer)
        {
            bool shouldSendRequest = true;
            
            if (asServer)
            {
                _lobby.OpenLobby();
                BindEvents();

                if (IsHost)
                {
                    shouldSendRequest = false;
                    ServiceContainer.Get<IUI>().ShowUI(ControllerIDs.LOBBY, true);
                    _lobby.RequestData();
                }
                
                return;
            }
            
            if(!shouldSendRequest)
                return;

            ServiceContainer.Get<IUI>().ShowUI(ControllerIDs.LOBBY, true);
            _lobby.RequestData();
        }

        public void Exit(bool asServer)
        {
            if (asServer)
            {
                IEnumerable<LobbyWaiter> _waiters = _lobby.FetchWaiters();
                _spawner.SpawnPlayers(_waiters);
                _lobby.CloseLobby();
                UnbindEvents();

                if (IsHost)
                {
                    ServiceContainer.Get<IUI>().HideUI(ControllerIDs.LOBBY);
                }
                
                return;
            }
            
            ServiceContainer.Get<IUI>().HideUI(ControllerIDs.LOBBY);
        }
    }
}
