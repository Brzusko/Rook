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
        

        private void InitializeOnce()
        {
            _stateMachine = _stateMachineGameObject.GetComponent<IStateMachine<MatchStatesID>>();
            _playersConsciousness = _playersConsciousnessGameObject.GetComponent<IPlayersConsciousness>();
        }

        public void Enter(bool asServer)
        {
            if (asServer)
            {
                _playersConsciousness.PossessAll();
                _stateMachine.ChangeState(MatchStatesID.GAMEPLAY, asServer);
            }
        }

        public void Exit(bool asServer)
        {
            if (asServer)
            {
                //TODO setup UIs
            }
        }
    }
}
