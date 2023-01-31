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
    public class PlayerStateMachine : NetworkBehaviour, IStateMachine<PlayerBaseStateID, PlayerStateMachineContext>
    {
        [Header("General")]
        [SerializeField] private CharacterMovement _characterMovement;
        [SerializeField] private PlayerInputReader _input;
        [SerializeField] private MovementStatsModule _movementStatsModule;
        [SerializeField] private TransformRotator _transformRotator;
        [SerializeField] private PlayerBaseStateID startingBaseState;
        [Header("Interfaces")]
        [SerializeField] private GameObject _playerEntityGameObject;
        [SerializeField] private GameObject _raycasterGameObject;
        [Range(0, 100)]
        [SerializeField] private uint _reconcileRate = 1;

        [SerializeField] private PlayerAnimations _playerAnimations;

        private Dictionary<PlayerBaseStateID, IState<NetworkInput>> _baseStates;
        private IState<NetworkInput> _currentBaseState;
        private PlayerBaseStateID _currentBaseStateID;
        private PlayerStateMachineContext _context;

        private IEntityToPossess _playerEntity;
        private IRaycaster _raycaster;
        public PlayerStateMachineContext Context => _context;

        private void Awake()
        {
            InitializeOnce();
            InitializeStateMachine();
            ChangeBaseState(startingBaseState);
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
            _context = new PlayerStateMachineContext(_characterMovement, _movementStatsModule, _transformRotator, _playerAnimations, _raycaster);
                
            _baseStates = new Dictionary<PlayerBaseStateID, IState<NetworkInput>>
            {
                { 
                    PlayerBaseStateID.IDLE, 
                    new MovementIdleState(this)
                },
                {
                    PlayerBaseStateID.WALKING,
                    new MovementWalkingState(this)
                },
                {
                    PlayerBaseStateID.SCUTTER,
                    new MovementScutterState(this)
                },
                {
                    PlayerBaseStateID.FALLING,
                    new MovementFallingState(this)
                },
                {
                    PlayerBaseStateID.JUMPING,
                    new MovementJumpState(this)
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
            _currentBaseState.Tick(input, asServer, isReplaying, (float)TimeManager.TickDelta);
            _currentBaseState.CheckStateChange(input);
        }
        
        [Reconcile]
        private void Reconcile(PlayerStateReconcileData data, bool asServer, Channel channel = Channel.Unreliable)
        {
            CharacterMovement.State state = new CharacterMovement.State(data.Position, data.Rotation, data.Velocity,
                data.IsConstrainedToGround, data.UnconstrainedTimer, data.HitGround, data.IsWalkable,
                data.GroundNormal);
            
            _movementStatsModule.SetAdditionalSpeedModifiers(data.AdditionalMovementMultiplier);
            _characterMovement.SetState(state);
            
            ChangeBaseState(data.BaseStateID);
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
                BaseStateID = _currentBaseStateID,
                AdditionalMovementMultiplier = _movementStatsModule.AdditionalSpeedModifiers,
            };
        }

        private void GenerateInput(out NetworkInput input)
        {
            input = default;
            
            RaycastHit hit = _raycaster.RaycastHit;
            Quaternion rotation = Quaternion.LookRotation(hit.point - transform.position);
            float yRotation = rotation.eulerAngles.y;
            
            if(yRotation == 0 && _input.MovementInput == default && !_input.IsJumpPressed)
                return;

            input.YRotation = yRotation;
            input.MovementInput = _input.MovementInput;
            input.IsWalkingPressed = _input.IsWalkingPressed;
            input.isJumpPressed = _input.IsJumpPressed;
        }

        #region Interfaces
        public void ChangeBaseState(PlayerBaseStateID baseStateID)
        {
            _currentBaseStateID = baseStateID;
            _currentBaseState?.Exit();

            if (!_baseStates.ContainsKey(baseStateID))
            {
                Debug.LogError("Could not find given state!");
                return;
            }
            
            _currentBaseState = _baseStates[_currentBaseStateID];
            _currentBaseState?.Enter();
        }
        #endregion
    }
}
