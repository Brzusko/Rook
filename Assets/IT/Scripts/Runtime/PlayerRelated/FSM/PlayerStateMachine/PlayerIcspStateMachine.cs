using System;
using System.Collections;
using System.Collections.Generic;
using EasyCharacterMovement;
using FishNet;
using FishNet.Connection;
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
    public class PlayerIcspStateMachine : NetworkBehaviour, ICSPStateMachine<PlayerBaseStateID, PlayerCombatStateID, PlayerStateMachineContext>
    {
        [Header("General")]
        [SerializeField] private CharacterMovement _characterMovement;
        [SerializeField] private PlayerInputReader _input;
        [SerializeField] private MovementStatsModule _movementStatsModule;
        [SerializeField] private TransformRotator _transformRotator;
        [SerializeField] private CombatModule _combatModule;
        [SerializeField] private PlayerBaseStateID _startingBaseState;
        [SerializeField] private PlayerCombatStateID _startCombatState;
        [Header("Interfaces")]
        [SerializeField] private GameObject _playerEntityGameObject;
        [SerializeField] private GameObject _raycasterGameObject;
        [Range(0, 100)]
        [SerializeField] private uint _reconcileRate = 1;
        [Range(0, 20)]
        [SerializeField] private uint _tickOffset = 2;
        [SerializeField] private int _snapshotsBufferLength = 1024;
        [SerializeField] private PlayerAnimations _playerAnimations;
        
        private Dictionary<PlayerBaseStateID, ICSPState<NetworkInput>> _baseStates;
        private Dictionary<PlayerCombatStateID, ICSPState<NetworkInput>> _combatStates;
        private ICSPState<NetworkInput> _currentBaseIcspState;
        private ICSPState<NetworkInput> _currentCombatIcspState;
        private PlayerBaseStateID _currentBaseStateID;
        private PlayerCombatStateID _currentCombatStateID;
        [SerializeField]
        private PlayerStateMachineContext _context;

        private IEntityToPossess _playerEntity;
        private IRaycaster _raycaster;
        
        private KnockbackCache _clientKnockbackCache;
        private KnockbackCache _serverKnockbackCache;
        private Vector3 _knockbackRequest;
        private Vector3 _knockbackForce;
        private uint _offset;
        
        private PlayerStateMachineSnapshot[] _snapshotsBuffer;
        
        public PlayerStateMachineContext Context => _context;
        public PlayerBaseStateID BaseStateID => _currentBaseStateID;
        public PlayerCombatStateID SecondaryStateID => _currentCombatStateID;

        private void Awake()
        {
            InitializeOnce();
            InitializeStateMachine();
            ChangeBaseState(_startingBaseState, false, false);
            ChangeSecondaryState(_startCombatState, false, false);
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

        public override void OnStartServer()
        {
            base.OnStartServer();

            _snapshotsBuffer = new PlayerStateMachineSnapshot[_snapshotsBufferLength];
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
            _context = new PlayerStateMachineContext(_characterMovement, _movementStatsModule, _transformRotator, _playerAnimations, _raycaster, _combatModule);
                
            _baseStates = new Dictionary<PlayerBaseStateID, ICSPState<NetworkInput>>
            {
                { 
                    PlayerBaseStateID.IDLE, 
                    new MovementIdleIcspState(this)
                },
                {
                    PlayerBaseStateID.SCUTTER,
                    new MovementScutterIcspState(this)
                },
                {
                    PlayerBaseStateID.FALLING,
                    new MovementFallingIcspState(this)
                },
                {
                    PlayerBaseStateID.JUMPING,
                    new MovementJumpIcspState(this)
                }
            };

            _combatStates = new Dictionary<PlayerCombatStateID, ICSPState<NetworkInput>>
            {
                {
                    PlayerCombatStateID.IDLE,
                    new IdleCombatIcspState(this)
                },
                {
                    PlayerCombatStateID.SWING,
                    new SwingCombatIcspState(this)
                },
                {
                    PlayerCombatStateID.PREPARE_SWING,
                    new PrepareSwingCombatIcspState(this)
                },
                {
                    PlayerCombatStateID.BLOCK,
                    new BlockCombatIcspState(this)
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
                
                SaveSnapshot();
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
            float deltaTime = (float)TimeManager.TickDelta;
            
            CacheKnockback(input, asServer);
            
            ProcessKnockback();
            
            CheckKnockback(input, asServer);
            
            _currentBaseIcspState.Tick(input, asServer, isReplaying, deltaTime);
            _currentCombatIcspState.Tick(input, asServer, isReplaying, deltaTime);
            _currentBaseIcspState.CheckStateChange(input, false, asServer, isReplaying);
            _currentCombatIcspState.CheckStateChange(input, false, asServer, isReplaying);
        }
        
        [Reconcile]
        private void Reconcile(PlayerStateReconcileData data, bool asServer, Channel channel = Channel.Unreliable)
        {
            _knockbackForce = data.KnockbackForce;
            CharacterMovement.State state = new CharacterMovement.State(data.Position, data.Rotation, data.Velocity,
                data.IsConstrainedToGround, data.UnconstrainedTimer, data.HitGround, data.IsWalkable,
                data.GroundNormal);
            
            _characterMovement.SetState(state);

            _context.CurrentPrepareSwingTime = data.CurrentPrepareSwingTime;
            _context.CurrentSwingTime = data.CurrentSwingTime;
            
            ChangeBaseState(data.BaseStateID, true, asServer);
            ChangeSecondaryState(data.CombatStateID, true, asServer);
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
                CombatStateID = _currentCombatStateID,
                CurrentPrepareSwingTime = _context.CurrentPrepareSwingTime,
                CurrentSwingTime = _context.CurrentSwingTime,
                KnockbackForce = _knockbackForce,
            };
        }

        private void GenerateInput(out NetworkInput input)
        {
            input = default;
            
            RaycastHit hit = _raycaster.RaycastHit;
            Quaternion rotation = Quaternion.LookRotation(hit.point - transform.position);
            float yRotation = rotation.eulerAngles.y;
            
            input.YRotation = yRotation;
            input.MovementInput = _input.MovementInput;
            input.IsWalkingPressed = _input.IsWalkingPressed;
            input.IsJumpPressed = _input.IsJumpPressed;
            input.IsMainActionPressed = _input.IsMainActionPressed;
            input.IsSecondaryActionPressed = _input.IsSecondaryActionPressed;
            input.Tick = TimeManager.LocalTick;
        }

        [TargetRpc]
        private void TargetCacheKnockback(NetworkConnection connection, uint tick, Vector3 deltaForce)
        {
            _clientKnockbackCache = new KnockbackCache
            {
                InvokeTime = tick,
                KnockbackForce = deltaForce
            };
        }

        private void CheckKnockback(NetworkInput input, bool asServer)
        {
            if (asServer && TimeManager.Tick == _serverKnockbackCache.InvokeTime)
            {
                _knockbackForce = _serverKnockbackCache.KnockbackForce;
            }
            
            if (!asServer && input.Tick == _clientKnockbackCache.InvokeTime)
            {
                _knockbackForce = _clientKnockbackCache.KnockbackForce;
            }
        }

        private void ProcessKnockback()
        {
            if (_knockbackForce == default) return;

            float _xComponent = _knockbackForce.x;
            float _yComponent = _knockbackForce.y;
            float _zComponent = _knockbackForce.z;
            float deceleration = 200;
            float dt = (float)TimeManager.TickDelta;

            _xComponent = Mathf.MoveTowards(_xComponent, 0, deceleration * dt);
            _yComponent = Mathf.MoveTowards(_yComponent, 0, deceleration * dt);
            _zComponent = Mathf.MoveTowards(_zComponent, 0, deceleration * dt);
            
            if(_characterMovement.isGrounded)
                _characterMovement.PauseGroundConstraint();
            
            _characterMovement.AddForce(_knockbackForce, ForceMode.Impulse);
            _knockbackForce = new Vector3(_xComponent, _yComponent, _zComponent);
        }
        
        private void CacheKnockback(NetworkInput input, bool asServer)
        { 
            if(_knockbackRequest == default)
                return;

            if (!asServer && IsHost && IsOwner)
            {
                uint hostTick = GenerateKnockbackTick(TimeManager.Tick);
                
                _clientKnockbackCache = new KnockbackCache
                {
                    InvokeTime = hostTick,
                    KnockbackForce = _knockbackRequest
                };

                ResetKnockbackRequest();
                
                return;
            }
            
            if(!asServer)
                return;

            uint serverTick = GenerateKnockbackTick(TimeManager.Tick);
            uint clientTick = GenerateKnockbackTick(input.Tick);
            
            _serverKnockbackCache = new KnockbackCache
            {
                InvokeTime = serverTick,
                KnockbackForce = _knockbackRequest
            };

            TargetCacheKnockback(Owner, clientTick, _knockbackRequest);
            ResetKnockbackRequest();
        }

        private uint GenerateKnockbackTick(uint localTick)
        {
            double tickDeltaMS = TimeManager.TickRate;
            double deduction = (long)(TimeManager.TickDelta * 1000d);
            double ping = Math.Max(0, Owner.Ping - deduction);

            uint pingAsTicks = (uint)Math.Round(ping / tickDeltaMS) + _tickOffset;
            
            return localTick + pingAsTicks + _offset;
        }

        private void ResetKnockbackRequest()
        {
            _knockbackRequest = default;
            _offset = default;
        }

        private void SaveSnapshot()
        {
            uint tick = TimeManager.LocalTick;
            int index = (int)tick % _snapshotsBufferLength;

            PlayerStateMachineSnapshot recentSnapshot = new PlayerStateMachineSnapshot
            {
                BaseStateID = _currentBaseStateID,
                PlayerCombatStateID = _currentCombatStateID,
                Tick = tick
            };

            _snapshotsBuffer[index] = recentSnapshot;
        }
    
        #region Interfaces
        public void ChangeBaseState(PlayerBaseStateID baseStateID, bool onReconcile, bool asServer, bool asReplay = false)
        {
            _currentBaseStateID = baseStateID;
            _currentBaseIcspState?.Exit(onReconcile, asReplay);

            if (!_baseStates.ContainsKey(baseStateID))
            {
                Debug.LogError("Could not find given state!");
                return;
            }
            
            _currentBaseIcspState = _baseStates[_currentBaseStateID];
            _currentBaseIcspState?.Enter(onReconcile, asServer, asReplay);
        }

        public void ChangeSecondaryState(PlayerCombatStateID stateID, bool onReconcile, bool asServer, bool asReplay = false)
        {
            _currentCombatStateID = stateID;
            _currentCombatIcspState?.Exit(onReconcile, asReplay);

            if (!_combatStates.ContainsKey(stateID))
            {
                Debug.LogError("Could not find given state!");
                return;
            }

            _currentCombatIcspState = _combatStates[_currentCombatStateID];
            _currentCombatIcspState?.Enter(onReconcile, asServer, asReplay);
        }

        public bool TryGetSnapshotAtTick(uint tick, out PlayerStateMachineSnapshot snapshot)
        {
            int index = (int)tick % _snapshotsBufferLength;
            snapshot = _snapshotsBuffer[index];
            return snapshot.Tick.Equals(tick);
        }

        public void RequestKnockback(Vector3 knockback, uint offset)
        {
            _knockbackRequest = knockback;
            _offset = offset;
        }
        #endregion
    }
}
