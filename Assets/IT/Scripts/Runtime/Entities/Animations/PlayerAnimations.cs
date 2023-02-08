using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;
using EasyCharacterMovement;
using IT.Data.Networking;
using IT.FSM;
using UnityEngine.Serialization;

namespace IT
{
    public class PlayerAnimations : NetworkBehaviour
    {
        [SerializeField] private Transform _mainSpaceTransform;
        [SerializeField] private Animator _animator;
        [SerializeField] private CharacterMovement _characterMovement;
        [FormerlySerializedAs("_stateMachine")] [SerializeField] private PlayerIcspStateMachine icspStateMachine;
        [SerializeField] private MovementStatsModule _movementStatsModule;

        [Header("Animation Rates")] 
        [SerializeField] private float _baseMaxValue;
        [SerializeField] private float _prepareSwingValue;
        [SerializeField] private float _blockValue;

        private Dictionary<PlayerMovementAnimID, int> _animationMovementHashes;
        private Dictionary<PlayerCombatAnimID, int> _animationCombatHashes;
        private int _xComponentHash;
        private int _zComponentHash;
        private int _jumpHash;
        private int _swingHash;
        
        private PlayerMovementAnimID _currentMovementAnimationID;
        private PlayerCombatAnimID _currentCombatAnimationID;

        private Vector2 _movementVector;

        private void Awake()
        {
            InitializeOnce();
        }

        private void Start()
        {
            TimeManager.OnUpdate += OnUpdate;
        }

        private void OnDestroy()
        {
            TimeManager.OnUpdate -= OnUpdate;
        }

        private void OnUpdate()
        {
            if(!IsOwner)
                return;
            
            ProcessParams();
        }

        private void ProcessParams()
        {
            Vector3 characterVelocity = _characterMovement.velocity;
            Vector3 transformedVelocity = _mainSpaceTransform.InverseTransformDirection(characterVelocity);
            Vector2 verticalVelocity = new Vector2(transformedVelocity.x, transformedVelocity.z);

            PlayerCombatStateID combatStateID = icspStateMachine.SecondaryStateID;
            
            float baseMaxValue = _baseMaxValue;
            baseMaxValue = combatStateID == PlayerCombatStateID.BLOCK ? _blockValue : baseMaxValue;
            baseMaxValue = combatStateID == PlayerCombatStateID.PREPARE_SWING ? _prepareSwingValue : baseMaxValue;
            
            verticalVelocity = Vector2.ClampMagnitude(verticalVelocity, baseMaxValue);

            float xComponent = _movementVector.x;
            float yComponent = _movementVector.y;
            float acceleration = _movementStatsModule.Acceleration;

            xComponent = Mathf.MoveTowards(xComponent, verticalVelocity.x, acceleration * Time.deltaTime);
            yComponent = Mathf.MoveTowards(yComponent, verticalVelocity.y, acceleration * Time.deltaTime);

            _movementVector = new Vector2(xComponent, yComponent);
            
            _animator.SetFloat(_xComponentHash, xComponent);
            _animator.SetFloat(_zComponentHash, yComponent);
        }

        private void InitializeOnce()
        {
            _xComponentHash = Animator.StringToHash("X");
            _zComponentHash = Animator.StringToHash("Z");
            _jumpHash = Animator.StringToHash("Jump");
            _swingHash = Animator.StringToHash("Swing");
            
            _animationMovementHashes = new Dictionary<PlayerMovementAnimID, int>
            {
                {
                    PlayerMovementAnimID.JUMPING, Animator.StringToHash("Jump Start")
                },
                {
                    PlayerMovementAnimID.FALLING, Animator.StringToHash("Jump Mid")
                },
                {
                    PlayerMovementAnimID.LANDING, Animator.StringToHash("Jump Land")
                },
                {
                    PlayerMovementAnimID.GROUNDED, Animator.StringToHash("Grounded Movement")
                }
            };

            _animationCombatHashes = new Dictionary<PlayerCombatAnimID, int>
            {
                {
                    PlayerCombatAnimID.NONE, Animator.StringToHash("Combat Idle")
                },
                {
                    PlayerCombatAnimID.PREPARE_SWING, Animator.StringToHash("Prepare Swing")
                },
                {
                    PlayerCombatAnimID.SWING, Animator.StringToHash("Swing")
                },
                {
                    PlayerCombatAnimID.BLOCK, Animator.StringToHash("Block")
                }
            };
        }

        public void PlayAnimation(PlayerMovementAnimID movementAnimID)
        {
            if(!_animationMovementHashes.ContainsKey(movementAnimID) || _currentMovementAnimationID == movementAnimID)
                return;

            _currentMovementAnimationID = movementAnimID;
            _animator.SetInteger(_jumpHash, (int)_currentMovementAnimationID);
        }

        public void PlayAnimation(PlayerCombatAnimID combatAnimID)
        {
            if(!_animationCombatHashes.ContainsKey(combatAnimID) || _currentCombatAnimationID == combatAnimID)
                return;

            _currentCombatAnimationID = combatAnimID;
            _animator.SetInteger(_swingHash, (int)combatAnimID);
        }
    }
}
