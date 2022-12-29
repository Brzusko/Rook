using System;
using System.Collections;
using System.Collections.Generic;
using EasyCharacterMovement;
using FishNet;
using FishNet.Managing.Timing;
using FishNet.Object;
using FishNet.Object.Prediction;
using IT.FSM.States;
using IT.Input;
using IT.Interfaces.FSM;
using Mono.CSharp;
using UnityEngine;

namespace IT.FSM
{
    public class MovementStateMachine : NetworkBehaviour, IStateMachine<MovementStateID, MovementContext>
    {
        [SerializeField] private CharacterMovement _characterMovement;
        [SerializeField] private PlayerInputReader _input;
        [SerializeField] private MovementStateID _startingState;

        private Dictionary<MovementStateID, IState<NetworkedInput>> _states;
        private MovementStateID _currentStateID;
        private IState<NetworkedInput> _currentState;
        private MovementContext _context;
        private TimeManager _timeManager;
        public MovementContext Context => _context;
        
        private void Awake()
        {
            InitializeStateMachine();
            ChangeState(_startingState);
        }

        private void Start()
        {
            _timeManager = InstanceFinder.TimeManager;
            
            _timeManager.OnTick += OnTick;
            _timeManager.OnPreTick += OnPreTick;
            //_timeManager.OnPostTick += OnTick;
        }

        private void OnDestroy()
        {
            _timeManager.OnTick -= OnTick;
            _timeManager.OnPreTick -= OnPreTick;
            //_timeManager.OnPostTick -= OnPostTick;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            
            _characterMovement.enabled = (base.IsServer || base.IsOwner);
        }
        
        private void InitializeStateMachine()
        {
            _context = new MovementContext(_characterMovement);
                
            _states = new Dictionary<MovementStateID, IState<NetworkedInput>>
            {
                {
                  MovementStateID.IDLE,
                  new MovementIdleState(this)
                },
                {
                    MovementStateID.MOVING,
                    new MovementMovingState(this)
                }
            };
        }

        private void OnPreTick()
        {
            if(IsOwner || IsServer)
                _currentState.CheckStateChange();
        }
        
        private void OnTick()
        {
            if (IsOwner)
            {
                Reconcile(default, false);
                NetworkedInput input = _input.NetworkedInput;
                Simulation(input, false);
            }

            if (IsServer)
            {
                Simulation(default, true);
                Reconcile(GenerateReconcileData(), true);
            }
        }

        private void OnPostTick()
        {
            if(!IsServer)
                return;
        }
        
        [Replicate]
        private void Simulation(NetworkedInput input, bool asServer, bool isReplaying = false)
        {
            _currentState.Tick(input, asServer, isReplaying, (float)_timeManager.TickDelta);
        }
        
        [Reconcile]
        private void Reconcile(ReconcileData data, bool asServer)
        {
            CharacterMovement.State state = new CharacterMovement.State(data.Position, data.Rotation, data.Velocity,
                data.IsConstrainedToGround, data.UnconstrainedTimer, data.HitGround, data.IsWalkable,
                data.GroundNormal);
            
            _characterMovement.SetState(state);
            ChangeState(data.StateID);
        }
        
        private ReconcileData GenerateReconcileData()
        {
            CharacterMovement.State state = _characterMovement.GetState();
    
            return new ReconcileData
            {
                Velocity = state.velocity,
                Position = state.position,
                Rotation = state.rotation,
                GroundNormal = state.groundNormal,
                IsConstrainedToGround = state.isConstrainedToGround,
                UnconstrainedTimer = state.unconstrainedTimer,
                HitGround = state.hitGround,
                IsWalkable = state.isWalkable,
                StateID = _currentStateID,
            };
        }

        private struct ReconcileData
        {
            public Vector3 Velocity;
            public Vector3 Position;
            public Vector3 GroundNormal;
            public Quaternion Rotation;
            public bool IsConstrainedToGround;
            public float UnconstrainedTimer;
            public bool HitGround;
            public bool IsWalkable;
            public MovementStateID StateID;
        }

        
        public void ChangeState(MovementStateID stateID)
        {
            _currentStateID = stateID;
            _currentState?.Exit();

            if (!_states.ContainsKey(stateID))
            {
                Debug.LogError("Could not find given state!");
                return;
            }
            
            _currentState = _states[_currentStateID];
            _currentState?.Enter();
        }
    }

    public class MovementContext
    {
        public MovementContext(CharacterMovement characterMovement)
        {
            CharacterMovement = characterMovement;
        }
        
        public CharacterMovement CharacterMovement { get; }
        public float MaxSpeed => 9f;
        public float Acceleration => 20f;
    }
}
