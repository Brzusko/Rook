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
using IT.Interfaces;
using IT.Interfaces.FSM;
using Mono.CSharp;
using UnityEngine;

namespace IT.FSM
{
    public class PlayerStateMachine : NetworkBehaviour, IStateMachine<PlayerStateID, MovementContext>
    {
        [SerializeField] private CharacterMovement _characterMovement;
        [SerializeField] private PlayerInputReader _input;
        [SerializeField] private PlayerStateID _startingState;
        [SerializeField] private GameObject _playerEntityGameObject;
        [SerializeField] private GameObject _raycasterGameObject;

        private Dictionary<PlayerStateID, IState<NetworkedInput>> _states;
        private PlayerStateID _currentStateID;
        private IState<NetworkedInput> _currentState;
        private MovementContext _context;
        private TimeManager _timeManager;
        
        private IEntityToPossess _playerEntity;
        private IRaycaster _raycaster;
        public MovementContext Context => _context;
        
        private void Awake()
        {
            InitializeOnce();
            InitializeStateMachine();
            ChangeState(_startingState);
        }

        private void Start()
        {
            _timeManager = InstanceFinder.TimeManager;
            
            _timeManager.OnTick += OnTick;
            _timeManager.OnPreTick += OnPreTick;
            _playerEntity.ClientPossessChanged += OnClientPossessChanged;
        }

        private void OnDestroy()
        {
            _timeManager.OnTick -= OnTick;
            _timeManager.OnPreTick -= OnPreTick;
            _playerEntity.ClientPossessChanged -= OnClientPossessChanged;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            CheckMovementComponentState();
        }

        private void InitializeOnce()
        {
            _playerEntity = _playerEntityGameObject.GetComponent<IEntityToPossess>();
            _raycaster = _raycasterGameObject.GetComponent<IRaycaster>();
        }

        private void CheckMovementComponentState()
        {
            _characterMovement.enabled = (base.IsServer || base.IsOwner);
        }

        private void InitializeStateMachine()
        {
            _context = new MovementContext(_characterMovement, _raycaster);
                
            _states = new Dictionary<PlayerStateID, IState<NetworkedInput>>
            {
                { 
                    PlayerStateID.IDLE, 
                    new PlayerIdleState(this)
                },
                {
                    PlayerStateID.MOVING,
                    new PlayerWalkingState(this)
                }
            };
        }

        #region Event Handlers

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

            if (!IsServer) return;
            
            Simulation(default, true);
            Reconcile(GenerateReconcileData(), true);
        }

        private void OnClientPossessChanged(bool possessionGained)
        {
            CheckMovementComponentState();
        }

        #endregion

        #region Client side prediction

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

        #endregion

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

        #region Interfaces
        public void ChangeState(PlayerStateID stateID)
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
        #endregion

        #region Structs
        
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
            public PlayerStateID StateID;
        }
        
        #endregion
    }

    public class MovementContext
    {
        public MovementContext(CharacterMovement characterMovement, IRaycaster raycaster)
        {
            CharacterMovement = characterMovement;
            Raycaster = raycaster;
        }
        
        public CharacterMovement CharacterMovement { get; }
        public IRaycaster Raycaster { get; }
        public float MaxSpeed => 9f;
        public float Acceleration => 20f;
    }
    
}
