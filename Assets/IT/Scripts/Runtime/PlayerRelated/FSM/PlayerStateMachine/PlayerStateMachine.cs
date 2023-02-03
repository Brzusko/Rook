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
    public class PlayerStateMachine : NetworkBehaviour, IStateMachine<PlayerBaseStateID, PlayerCombatStateID, PlayerStateMachineContext>
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

        [SerializeField] private bool _shouldIgnoreReplay;
        [SerializeField] private PlayerAnimations _playerAnimations;
        
        private Dictionary<PlayerBaseStateID, IState<NetworkInput>> _baseStates;
        private Dictionary<PlayerCombatStateID, IState<NetworkInput>> _combatStates;
        private IState<NetworkInput> _currentBaseState;
        private IState<NetworkInput> _currentCombatState;
        private PlayerBaseStateID _currentBaseStateID;
        private PlayerCombatStateID _currentCombatStateID;
        [SerializeField]
        private PlayerStateMachineContext _context;

        private IEntityToPossess _playerEntity;
        private IRaycaster _raycaster;
        
        private KnockbackCache _clientKnockbackCache;
        private KnockbackCache _serverKnockbackCache;
        private PlayerStateMachineSnapshot[] _snapshotsBuffer;
        
        public PlayerStateMachineContext Context => _context;
        public PlayerBaseStateID BaseStateID => _currentBaseStateID;
        public PlayerCombatStateID SecondaryStateID => _currentCombatStateID;

        private void Awake()
        {
            InitializeOnce();
            InitializeStateMachine();
            ChangeBaseState(_startingBaseState, false);
            ChangeSecondaryState(_startCombatState, false);
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
                
            _baseStates = new Dictionary<PlayerBaseStateID, IState<NetworkInput>>
            {
                { 
                    PlayerBaseStateID.IDLE, 
                    new MovementIdleState(this)
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

            _combatStates = new Dictionary<PlayerCombatStateID, IState<NetworkInput>>
            {
                {
                    PlayerCombatStateID.IDLE,
                    new IdleCombatState(this)
                },
                {
                    PlayerCombatStateID.SWING,
                    new SwingCombatState(this)
                },
                {
                    PlayerCombatStateID.PREPARE_SWING,
                    new PrepareSwingCombatState(this)
                },
                {
                    PlayerCombatStateID.BLOCK,
                    new BlockCombatState(this)
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

            CheckKnockback(input, asServer, isReplaying);
            
            _currentBaseState.Tick(input, asServer, isReplaying, deltaTime);
            _currentCombatState.Tick(input, asServer, isReplaying, deltaTime);
            _currentBaseState.CheckStateChange(input, false, isReplaying);
            _currentCombatState.CheckStateChange(input, false, isReplaying);
        }
        
        [Reconcile]
        private void Reconcile(PlayerStateReconcileData data, bool asServer, Channel channel = Channel.Unreliable)
        {
            CharacterMovement.State state = new CharacterMovement.State(data.Position, data.Rotation, data.Velocity,
                data.IsConstrainedToGround, data.UnconstrainedTimer, data.HitGround, data.IsWalkable,
                data.GroundNormal);
            
            _characterMovement.SetState(state);

            _context.CurrentPrepareSwingTime = data.CurrentPrepareSwingTime;
            _context.CurrentSwingTime = data.CurrentSwingTime;
            
            ChangeBaseState(data.BaseStateID, true);
            ChangeSecondaryState(data.CombatStateID, true);
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
            input.SimulationTick = TimeManager.LocalTick;
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

        private void CheckKnockback(NetworkInput input, bool asServer, bool isReplaying)
        {
            if (isReplaying && _shouldIgnoreReplay) return ;

            if (IsHost && TimeManager.LocalTick >= _serverKnockbackCache.InvokeTime && IsHost && IsOwner)
            {
                LaunchCharacter(_serverKnockbackCache.KnockbackForce);
                _serverKnockbackCache = default;
                return;
            }
            
            if (asServer && TimeManager.LocalTick == _serverKnockbackCache.InvokeTime)
            {
                LaunchCharacter(_serverKnockbackCache.KnockbackForce);
            }
            
            if (!asServer && input.SimulationTick == _clientKnockbackCache.InvokeTime)
            {
                LaunchCharacter(_clientKnockbackCache.KnockbackForce);
            }
        }

        private void LaunchCharacter(Vector3 force)
        {
            if(force == default)
                return;

            if(_characterMovement.isGrounded)
                _characterMovement.PauseGroundConstraint();
            
            _characterMovement.LaunchCharacter(force, true);
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
        public void ChangeBaseState(PlayerBaseStateID baseStateID, bool onReconcile, bool asReplay = false)
        {
            _currentBaseStateID = baseStateID;
            _currentBaseState?.Exit(onReconcile, asReplay);

            if (!_baseStates.ContainsKey(baseStateID))
            {
                Debug.LogError("Could not find given state!");
                return;
            }
            
            _currentBaseState = _baseStates[_currentBaseStateID];
            _currentBaseState?.Enter(onReconcile, asReplay);
        }

        public void ChangeSecondaryState(PlayerCombatStateID stateID, bool onReconcile, bool asReplay = false)
        {
            _currentCombatStateID = stateID;
            _currentCombatState?.Exit(onReconcile, asReplay);

            if (!_combatStates.ContainsKey(stateID))
            {
                Debug.LogError("Could not find given state!");
                return;
            }

            _currentCombatState = _combatStates[_currentCombatStateID];
            _currentCombatState?.Enter(onReconcile, asReplay);
        }

        public bool TryGetSnapshotAtTick(uint tick, out PlayerStateMachineSnapshot snapshot)
        {
            int index = (int)tick % _snapshotsBufferLength;
            snapshot = _snapshotsBuffer[index];
            return snapshot.Tick.Equals(tick);
        }
        
        public void CacheKnockback(Vector3 deltaForce, uint tickDelta)
        {
            double tickDeltaMS = TimeManager.TickRate;
            double deduction = (long)(TimeManager.TickDelta * 1000d);
            double ping = Math.Max(0, Owner.Ping - deduction);

            uint pingAsTicks = (uint)Math.Round(ping / tickDeltaMS) + _tickOffset;
            
            uint serverTick = TimeManager.Tick + pingAsTicks + tickDelta;
            uint clientTickWithOffset = Owner.LastPacketTick + pingAsTicks + tickDelta;

            _serverKnockbackCache = new KnockbackCache
            {
                InvokeTime = serverTick,
                KnockbackForce = deltaForce
            };
            
            if(IsHost && IsOwner)
                return;
            
            TargetCacheKnockback(Owner, clientTickWithOffset, deltaForce);
        }

        #endregion
    }
}
