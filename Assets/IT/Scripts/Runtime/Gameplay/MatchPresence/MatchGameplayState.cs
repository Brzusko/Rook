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
            
            _contestArea.PointCounterStartContesting += OnPointCounterStartContesting;
            _contestArea.PointCounterGainAllPoints += OnPointCounterGainAllPoints;
            
            ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
            
            _areEventsBound = true;
        }
        
        private void UnbindEvents()
        {
            if(!_areEventsBound)
                return;
            
            _contestArea.PointCounterStartContesting -= OnPointCounterStartContesting;
            _contestArea.PointCounterGainAllPoints -= OnPointCounterGainAllPoints;
            
            ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;
            
            _areEventsBound = false;
        }
        
        private void OnRemoteConnectionState(NetworkConnection arg1, RemoteConnectionStateArgs arg2)
        {
            throw new NotImplementedException();
        }

        private void OnPointCounterGainAllPoints(IPointCounter obj)
        {
            throw new NotImplementedException();
        }

        private void OnPointCounterStartContesting(IPointCounter obj)
        {
            throw new NotImplementedException();
        }

        public void Enter(bool asServer)
        {
            if (asServer)
            {
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
