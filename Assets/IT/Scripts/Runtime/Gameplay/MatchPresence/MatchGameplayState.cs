using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Transporting;
using IT.Interfaces;
using IT.Interfaces.FSM;
using IT.UI;
using UnityEngine;

namespace IT.Gameplay
{
    public class MatchGameplayState : NetworkBehaviour, IState<MatchStatesID>
    {
        [SerializeField] private GameObject _stateMachineGameObject;
        [SerializeField] private GameObject _contestAreaGameObject;
        public MatchStatesID StateID => MatchStatesID.GAMEPLAY;

        private IStateMachine<MatchStatesID> _stateMachine;
        private IContestArea _contestArea;

        private bool _areEventsBound;

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
            _contestArea = _contestAreaGameObject.GetComponent<IContestArea>();
        }

        private void BindEvents()
        {
            if(_areEventsBound)
                return;
            
            _contestArea.PointCounterGainAllPoints += OnPointCounterGainAllPoints;
            
            ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
            
            _areEventsBound = true;
        }
        
        private void UnbindEvents()
        {
            if(!_areEventsBound)
                return;
            
            _contestArea.PointCounterGainAllPoints -= OnPointCounterGainAllPoints;
            
            ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;
            _areEventsBound = false;
        }
        
        private void OnRemoteConnectionState(NetworkConnection arg1, RemoteConnectionStateArgs arg2)
        {
            if(arg2.ConnectionState != RemoteConnectionState.Stopped)
                return;
            
            if(ServerManager.Clients.Count > 1)
                return;

            if (ServerManager.Clients.Count == 1)
            {
                NetworkConnection connection = ServerManager.Clients.Values.First();
                
                if(!connection.Disconnecting)
                    return;
            }
            
            _stateMachine.ChangeState(MatchStatesID.WAITING, true);
        }

        private void OnPointCounterGainAllPoints(IPointCounter obj)
        {
            //cache result and send it on Exit
            
            _stateMachine.ChangeState(MatchStatesID.WON, true);
        }
        
        public void Enter(bool asServer)
        {
            if (asServer)
            {
                BindEvents();
            }
            
            ServiceContainer.Get<IUI>().ShowUI(ControllerIDs.GAMEPLAY);
        }

        public void Exit(bool asServer)
        {
            if (asServer)
            {
                UnbindEvents();
                
                _contestArea.Restart();
            }
            
            ServiceContainer.Get<IUI>().HideUI(ControllerIDs.GAMEPLAY);
        }
    }
}
