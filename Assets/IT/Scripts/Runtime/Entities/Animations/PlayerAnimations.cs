using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;
using Animancer;
using EasyCharacterMovement;
using FishNet.Managing.Timing;
using IT.Collections;
using IT.Data.Networking;
using UnityEditor.Animations;

public class PlayerAnimations : NetworkBehaviour
{
    [Header("Components")]
    [SerializeField] private AnimancerComponent _animancerComponent;
    [SerializeField] private Transform _playerMainSpaceTransform;
    [Header("Animation Assets")]
    [SerializeField] private MixerTransition2DAsset.UnShared _basicGroundMixer;
    [SerializeField] private ClipTransitionAsset.UnShared _fallingAnimation;
    [SerializeField] private ClipTransitionAsset.UnShared _landingAnimation;
    
    [Range(0.00f, 100f)]
    [SerializeField]
    private float _interpolationSpeed = 0.1f;
    
    [Range(1, 1000)]
    [SerializeField] private uint _sendRate;

    private Dictionary<PlayerAnimationStateID, AnimancerState> _animationStates;
    private PlayerAnimationStateID _currentAnimancerStateID;
    private bool _areEventsBound;
    private NetworkInput _cachedInput;
    private Vector2 _animationVector = Vector2.zero;
    private float _cachedAcceleration;
    private float _cachedDeceleration;
    private PlayerAnimationState _currentState;
    private PlayerAnimationState _lastState;
    private Vector2 _puppetAnimationVector;
    private readonly RingBuffer<PlayerAnimationState> _puppetAnimationStates = new RingBuffer<PlayerAnimationState>(1024);
    private float _lerpValue = 0.0f;

    private void Awake()
    {
        BuildAnimator();
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        BindEvents();
        PlayAnimation(PlayerAnimationStateID.GROUNDED);
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();
        UnbindEvents();
    }

    private void BuildAnimator()
    {
        _animationStates = new Dictionary<PlayerAnimationStateID, AnimancerState>
        {
            {
                PlayerAnimationStateID.GROUNDED, _animancerComponent.States.GetOrCreate(_basicGroundMixer)
            },
            {
                PlayerAnimationStateID.FALLING, _animancerComponent.States.GetOrCreate(_fallingAnimation)
            },
            {
                PlayerAnimationStateID.LANDING, _animancerComponent.States.GetOrCreate(_landingAnimation)
            }
        };
    }

    private void BindEvents()
    {
        if(_areEventsBound)
            return;

        base.TimeManager.OnUpdate += OnUpdate;
        base.TimeManager.OnPreTick += OnPreTick;
        base.TimeManager.OnPostTick += OnPostTick;
        _areEventsBound = true;
    }

    private void UnbindEvents()
    {
        if(!_areEventsBound)
            return;

        base.TimeManager.OnUpdate -= OnUpdate;
        base.TimeManager.OnPreTick -= OnPreTick;
        base.TimeManager.OnPostTick -= OnPostTick;
        _areEventsBound = true;
    }

    private void OnUpdate()
    {
        if(PredictionManager.IsReplaying())
            return;
        
        if (!IsOwner && !IsServer)
        {
            ProcessAnimationPuppet();
            return;
        }
        
        ProcessGroundAnimation();
    }

    private void OnPreTick()
    {
        if (!IsOwner && !IsServer)
        {
            GrabAnimationStates();
        }
    }
    
    private void OnPostTick()
    {
        if (!IsServer) return;
        uint currentTick = TimeManager.LocalTick;
        
        if((currentTick % _sendRate) != 0)
            return;

        float speed = _animationStates[_currentAnimancerStateID].Speed;
        float duration = _animationStates[_currentAnimancerStateID].Duration;
        
        Server_SendAnimationState(new PlayerAnimationState{ AnimationVector = _animationVector, Tick = currentTick, Speed = speed, Duration = duration, StateID = _currentAnimancerStateID});
    }

    public void CacheInput(NetworkInput input, float acceleration, float deceleration)
    {
        _cachedInput = input;
        _cachedAcceleration = acceleration;
        _cachedDeceleration = deceleration;
    }

    public void PlayAnimation(PlayerAnimationStateID animID)
    {
        if(_animationStates == null || !_animationStates.ContainsKey(animID))
            return;
        
        if(animID == _currentAnimancerStateID)
            return;
        
        _currentAnimancerStateID = animID;

        _animancerComponent.Play(_animationStates[animID], 0.25f);
    }
    
    [ObserversRpc(BufferLast = true, IncludeOwner = false)]
    private void Server_SendAnimationState(PlayerAnimationState animationState)
    {
        if(IsHost)
            return;

        if (_puppetAnimationStates.Count == 0)
        {
            _puppetAnimationStates.Write(animationState);
            return;
        }

        if(_puppetAnimationStates.Peek().Tick > animationState.Tick)
            return;
        
        _puppetAnimationStates.Write(animationState);
    }

    private void ProcessGroundAnimation()
    {
        if(_currentAnimancerStateID != PlayerAnimationStateID.GROUNDED)
            return;
        
        Vector3 transformedVelocity =
            _playerMainSpaceTransform.InverseTransformDirection(new Vector3(_cachedInput.MovementInput.x, 0,
                _cachedInput.MovementInput.y));
        
        float animationScalar = _cachedInput.IsWalkingPressed ? 1f : 1.7f;
        bool isPlayerMoving = transformedVelocity.sqrMagnitude > 0f;
        float targetX = isPlayerMoving ? transformedVelocity.x * animationScalar : 0;
        float targetY = isPlayerMoving ? transformedVelocity.z * animationScalar : 0;
        float maxDelta = isPlayerMoving
            ? _cachedAcceleration * Time.deltaTime
            : _cachedDeceleration * Time.deltaTime;

        float desiredX = Mathf.MoveTowards(_animationVector.x, targetX, maxDelta);
        float desiredY = Mathf.MoveTowards(_animationVector.y, targetY, maxDelta);

        _animationVector = new Vector2(desiredX, desiredY);
        ((MixerState<Vector2>)_animationStates[_currentAnimancerStateID]).Parameter = _animationVector;
    }
    
    //TODO process one-shot animations
    private void ProcessAnimationPuppet()
    {
        if (_lastState.StateID != _currentState.StateID)
        {
            PlayAnimation(_currentState.StateID);    
        }
        
        if(_currentAnimancerStateID != PlayerAnimationStateID.GROUNDED)
            return;

        _lerpValue += Time.deltaTime * _interpolationSpeed;
        _puppetAnimationVector = Vector2.Lerp(_puppetAnimationVector, _currentState.AnimationVector,
            _lerpValue);
        
        ((MixerState<Vector2>)_animationStates[_currentAnimancerStateID]).Parameter = _puppetAnimationVector;
    }

    private void GrabAnimationStates()
    {
        while (_puppetAnimationStates.Count > 1)
        {
            _lastState = _currentState;
            _currentState = _puppetAnimationStates.Read();
            Debug.Log($"{_lastState.StateID.ToString()}, {_currentState.StateID.ToString()}");
            _lerpValue = 0;
        }
    }
}
