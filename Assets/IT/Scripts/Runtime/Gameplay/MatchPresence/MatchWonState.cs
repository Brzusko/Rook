using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using IT.Gameplay;
using IT.Interfaces.FSM;
using IT.Utils;
using UnityEngine;

public class MatchWonState : NetworkBehaviour, IState<MatchStatesID>
{
    [SerializeField] private GameObject _stateMachineGameObject;
    [SerializeField] private float _timeToChangeStateInSec = 10f;

    private IStateMachine<MatchStatesID> _stateMachine;
    private bool _tickEventsBound;
    [SerializeField]
    private SimpleTimer _timer = new();
    
    public MatchStatesID StateID => MatchStatesID.WON;

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
    }

    private void BindTickEvents()
    {
        if(_tickEventsBound)
            return;

        TimeManager.OnTick += OnTick;
        _timer.Complete += OnTimerComplete;
        _tickEventsBound = true;
    }

    private void UnbindTickEvents()
    {
        if(!_tickEventsBound)
            return;

        TimeManager.OnTick -= OnTick;
        _timer.Complete -= OnTimerComplete;
        _tickEventsBound = false;
    }

    private void OnTick()
    {
        float deltaTime = (float)TimeManager.TickDelta;
        _timer.Update(deltaTime);
    }

    private void OnTimerComplete()
    {
        _stateMachine.ChangeState(MatchStatesID.WAITING, true);
    }

    public void Enter(bool asServer)
    {
        if (asServer)
        {
            _timer.Start(_timeToChangeStateInSec);
            Debug.Log("Won state change");
            BindTickEvents();
        }
    }

    public void Exit(bool asServer)
    {
        if (asServer)
        {
            Debug.Log("Won state exit");
            UnbindTickEvents();
        }
    }
}
