using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using IT.Interfaces.FSM;
using UnityEngine;

namespace IT.Gameplay
{
    public class MatchStateMachine : NetworkBehaviour, IStateMachine<MatchStatesID>
    {
        [SerializeField] private GameObject[] _stateGameObjects;
        [SerializeField] private MatchStatesID _startState;
        
        private Dictionary<MatchStatesID, IState<MatchStatesID>> _states;
        private IState<MatchStatesID> _currentState;

        private void Awake() => InitializeOnce();

        public override void OnStartServer()
        {
            base.OnStartServer();
            ChangeState(_startState, true);
        }

        private void InitializeOnce()
        {
            _states = new Dictionary<MatchStatesID, IState<MatchStatesID>>();
            
            foreach (GameObject stateGameObject in _stateGameObjects)
            {
                if(!stateGameObject.TryGetComponent(out IState<MatchStatesID> state))
                    continue;
                
                if(_states.ContainsKey(state.StateID))
                    continue;
                
                _states.Add(state.StateID, state);
            }
        }
        
        public void ChangeState(MatchStatesID stateID, bool asServer)
        {
            if(!_states.ContainsKey(stateID))
                return;
            
            _currentState?.Exit(asServer);
            _currentState = _states[stateID];
            _currentState?.Enter(asServer);
            
            if(!asServer)
                return;
            
            ObserverChangeState(stateID);
        }
        
        [ObserversRpc]
        private void ObserverChangeState(MatchStatesID stateID)
        {
            ChangeState(stateID, false);
        }
    }
}
