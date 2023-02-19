using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Transporting;
using IT.Interfaces;
using IT.Interfaces.FSM;
using IT.UI;
using IT.Utils;
using UnityEngine;

namespace IT.Gameplay
{
    public class MatchPreparingState : NetworkBehaviour, IState<MatchStatesID>
    {
        [SerializeField] private GameObject _stateMachineGameObject;
        [SerializeField] private GameObject _playersConsciousnessGameObject;
        
        private IStateMachine<MatchStatesID> _stateMachine;
        private IPlayersConsciousness _playersConsciousness;
        private SimpleTimer _timer = new();

        private bool _areTickEventsBound;
        
        public MatchStatesID StateID => MatchStatesID.PREPARING;

        private void Awake()
        {
            InitializeOnce();
        }

        private void OnDestroy()
        {
            UnbindTickEvents();
        }

        private void InitializeOnce()
        {
            _stateMachine = _stateMachineGameObject.GetComponent<IStateMachine<MatchStatesID>>();
            _playersConsciousness = _playersConsciousnessGameObject.GetComponent<IPlayersConsciousness>();
        }

        private void BindTickEvents()
        {
            if(_areTickEventsBound)
                return;

            TimeManager.OnTick += OnTick;
            _timer.Complete += OnTimerComplete;
            
            _areTickEventsBound = true;
        }

        private void UnbindTickEvents()
        {
            if(!_areTickEventsBound)
                return;

            TimeManager.OnTick -= OnTick;
            _timer.Complete -= OnTimerComplete;
            
            _areTickEventsBound = false;
        }

        private void OnTick()
        {
            float deltaTime = (float)TimeManager.TickDelta;
            _timer.Update(deltaTime);
        }

        private void OnTimerComplete()
        {
            _stateMachine.ChangeState(MatchStatesID.GAMEPLAY, true);
        }

        public void Enter(bool asServer)
        {
            if (asServer)
            {
                _playersConsciousness.PossessAll();
                _timer.Start(1f);
                
                BindTickEvents();
            }
        }

        public void Exit(bool asServer)
        {
            if (asServer)
            {
                UnbindTickEvents();
            }
        }
    }
}
