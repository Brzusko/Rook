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

        private uint _serverTick;
        private uint _clientTick;
        private uint _lastInputTick;

        private List<System.Tuple<uint, Vector3>> _knockbackCache = new List<System.Tuple<uint, Vector3>>();

        private Vector3 _deltaForce;
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
            _playerAnimations.CacheInput(input, _movementStatsModule.Acceleration, _movementStatsModule.Deceleration);

            if (asServer)
                _lastInputTick = input.SimulationTick;

            // if (isReplaying && !asServer && _knockbackCache.Count > 0)
            // {
            //     System.Tuple<uint, Vector3> knockbackToRemove = null;
            //     
            //     foreach (System.Tuple<uint, Vector3> knockback in _knockbackCache)
            //     {
            //         if (knockback.Item1 == input.SimulationTick)
            //         {
            //             knockbackToRemove = knockback;
            //             break;
            //         }
            //     }
            //
            //     if (knockbackToRemove != null)
            //     {
            //         LaunchCharacter(knockbackToRemove.Item2);
            //         _knockbackCache.Remove(knockbackToRemove);
            //     }
            // }
            
            CheckDeltaForce(input, asServer, isReplaying);
            
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

            // if (!asServer)
            // {
            //     Vector3 clientPosition = _characterMovement.GetPosition();
            //     Vector3 velocity = _characterMovement.velocity;
            //     bool isGrounded = _characterMovement.isConstrainedToGround;
            //     
            //     Debug.Log($"Before recon: {clientPosition}, {velocity}, {isGrounded}");
            //     Debug.Log($"After recon: {data.Position}, {data.Velocity}, {data.IsConstrainedToGround}");
            // }
            
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
            
            // if(yRotation == 0 
            //    && _input.MovementInput == default 
            //    && !_input.IsJumpPressed
            //    && !_input.IsMainActionPressed
            //    && !_input.IsSecondaryActionPressed
            //    )
            //     return;

            input.YRotation = yRotation;
            input.MovementInput = _input.MovementInput;
            input.IsWalkingPressed = _input.IsWalkingPressed;
            input.IsJumpPressed = _input.IsJumpPressed;
            input.IsMainActionPressed = _input.IsMainActionPressed;
            input.IsSecondaryActionPressed = _input.IsSecondaryActionPressed;
            input.SimulationTick = TimeManager.LocalTick;

            if (_deltaForce != default)
            {
                input.DeltaForce = _deltaForce;
                _deltaForce = default;
            }

            if (_clientTick != default)
            {
                input.KnockbackTick = _clientTick;
                _clientTick = default;
            }
        }

        public void CacheDeltaForce(Vector3 deltaForce, uint tickDelta)
        {
            double tickDeltaMS = TimeManager.TickDelta * 1000d;
            double ping = Owner.Ping / 2f;

            uint pingAsTicks = (uint)Math.Round(ping / tickDeltaMS);
            
            _serverTick = TimeManager.Tick + tickDelta;
            uint clientTickWithOffset = Owner.LastPacketTick + pingAsTicks + tickDelta;
            
            _deltaForce = deltaForce;
            
            Debug.Log($"Serv tick {_serverTick}, Cl local tick {Owner.LocalTick}, Cl offset tick {clientTickWithOffset}");
            TargetCacheDeltaForce(Owner, clientTickWithOffset, deltaForce);
        }

        [TargetRpc]
        private void TargetCacheDeltaForce(NetworkConnection connection, uint tick, Vector3 deltaForce)
        {
            _deltaForce = deltaForce;
            _clientTick = tick;
            
            Debug.Log($"Local tick {TimeManager.LocalTick}, Received tick {tick}");
        }

        private void CheckDeltaForce(NetworkInput input, bool asServer, bool isReplaying)
        {
            if (asServer && TimeManager.Tick == _serverTick)
            {
                LaunchCharacter(_deltaForce);
                _deltaForce = default;
            }

            if (!asServer && input.SimulationTick >= _clientTick)
            {
                //_knockbackCache.Add(new System.Tuple<uint, Vector3>(input.SimulationTick, input.DeltaForce));
                LaunchCharacter(input.DeltaForce * 2);
            }
        }

        private void LaunchCharacter(Vector3 force)
        {
            if(force == default)
                return;

            if(_characterMovement.isGrounded)
                _characterMovement.PauseGroundConstraint();
            
            _characterMovement.LaunchCharacter(force);
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

        #endregion
    }
}
