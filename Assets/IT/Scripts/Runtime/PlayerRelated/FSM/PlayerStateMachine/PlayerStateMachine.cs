using System;
using System.Collections;
using System.Collections.Generic;
using EasyCharacterMovement;
using FishNet;
using FishNet.Managing.Timing;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using IT.Data.Networking;
using IT.FSM.States;
using IT.Input;
using IT.Interfaces;
using IT.Interfaces.FSM;
using Mono.CSharp;
using UnityEngine;
using UnityEngine.InputSystem;

namespace IT.FSM
{
    public class PlayerStateMachine : NetworkBehaviour, IStateMachine<PlayerStateID, MovementContext>
    {
        [Header("General")]
        [SerializeField] private CharacterMovement _characterMovement;
        [SerializeField] private PlayerInputReader _input;
        [SerializeField] private MovementStatsModule _movementStatsModule;
        [SerializeField] private TransformRotator _transformRotator;
        [SerializeField] private PlayerStateID _startingState;
        [Header("Interfaces")]
        [SerializeField] private GameObject _playerEntityGameObject;
        [SerializeField] private GameObject _raycasterGameObject;
        [Range(0, 100)]
        [SerializeField] private uint _reconcileRate = 1;

        [SerializeField] private PlayerAnimations _playerAnimations;

        private Dictionary<PlayerStateID, IState<NetworkInput>> _states;
        private PlayerStateID _currentStateID;
        private IState<NetworkInput> _currentState;
        private MovementContext _context;

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
            TimeManager.OnTick += OnTick;
            _playerEntity.ClientPossessChanged += OnClientPossessChanged;
        }

        private void OnDestroy()
        {
            TimeManager.OnTick -= OnTick;
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
            _context = new MovementContext(_characterMovement, _movementStatsModule, _transformRotator, _playerAnimations, _raycaster);
                
            _states = new Dictionary<PlayerStateID, IState<NetworkInput>>
            {
                { 
                    PlayerStateID.IDLE, 
                    new PlayerIdleState(this)
                },
                {
                    PlayerStateID.WALKING,
                    new PlayerWalkingState(this)
                },
                {
                    PlayerStateID.SCUTTER,
                    new PlayerScutterState(this)
                },
                {
                    PlayerStateID.FALLING,
                    new PlayerFallingState(this)
                }
            };
        }

        #region Event Handlers

        private void OnTick()
        {
            if (IsOwner)
            {
                Reconcile(default, false);

                GenerateInput(out NetworkInput input);
                
                Simulation(input, false);
            }

            if (IsServer)
            {
                Simulation(default, true);

                uint tick = base.TimeManager.LocalTick;

                if ((tick % _reconcileRate) == 0)
                {
                    Reconcile(GenerateReconcileData(), true);
                }
            }
        }

        private void OnClientPossessChanged(bool possessionGained)
        {
            CheckMovementComponentState();
        }

        #endregion

        #region Client side prediction

        [Replicate]
        private void Simulation(NetworkInput input, bool asServer, Channel channel = Channel.Unreliable, bool isReplaying = false)
        {
            _playerAnimations.CacheInput(input, _movementStatsModule.Acceleration, _movementStatsModule.Deceleration);
            _currentState.Tick(input, asServer, isReplaying, (float)TimeManager.TickDelta);
            _currentState.CheckStateChange(input);
        }
        
        [Reconcile]
        private void Reconcile(PlayerStateReconcileData data, bool asServer, Channel channel = Channel.Unreliable)
        {
            CharacterMovement.State state = new CharacterMovement.State(data.Position, data.Rotation, data.Velocity,
                data.IsConstrainedToGround, data.UnconstrainedTimer, data.HitGround, data.IsWalkable,
                data.GroundNormal);
            
            _movementStatsModule.SetAdditionalSpeedModifiers(data.AdditionalMovementMultiplier);
            _characterMovement.SetState(state);
            
            ChangeState(data.StateID);
        }

        #endregion

        private PlayerStateReconcileData GenerateReconcileData()
        {
            CharacterMovement.State state = _characterMovement.GetState();
    
            return new PlayerStateReconcileData
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
                AdditionalMovementMultiplier = _movementStatsModule.AdditionalSpeedModifiers,
            };
        }

        private void GenerateInput(out NetworkInput input)
        {
            input = default;
            
            RaycastHit hit = _raycaster.RaycastHit;
            Quaternion rotation = Quaternion.LookRotation(hit.point - transform.position);
            float yRotation = rotation.eulerAngles.y;
            
            if(yRotation == 0 || _input.MovementInput == default)
                return;

            input.YRotation = yRotation;
            input.MovementInput = _input.MovementInput;
            input.IsWalkingPressed = _input.IsWalkingPressed;
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
    }

    public class MovementContext
    {
        public MovementContext(CharacterMovement characterMovement, MovementStatsModule movementStatsModule, TransformRotator rotator, PlayerAnimations playerAnimations, IRaycaster raycaster)
        {
            CharacterMovement = characterMovement;
            MovementStatsModule = movementStatsModule;
            Rotator = rotator;
            PlayerAnimations = playerAnimations;
        }
        
        public CharacterMovement CharacterMovement { get; }
        public MovementStatsModule MovementStatsModule { get; }
        public TransformRotator Rotator { get; }
        public PlayerAnimations PlayerAnimations { get; }
    }
    
}
