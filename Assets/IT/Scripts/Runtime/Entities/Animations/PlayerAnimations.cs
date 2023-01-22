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
    
    [Range(0.00f, 100f)]
    [SerializeField]
    private float _interpolationSpeed = 0.1f;

    private MixerState<Vector2> _basicGroundState;
    private bool _areEventsBound;

    private NetworkInput _cachedInput;
    private Vector2 _animationVector = Vector2.zero;
    private float _cachedAcceleration;
    private float _cachedDeceleration;
    private PlayerAnimationState _currentState;
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
        
        _animancerComponent.Play(_basicGroundState);
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();
        UnbindEvents();
    }

    private void BuildAnimator()
    {
        _basicGroundState = (MixerState<Vector2>)_animancerComponent.States.GetOrCreate(_basicGroundMixer);
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

        if(!_basicGroundState.IsPlaying)
            return;
        
        if (!IsOwner && !IsServer)
        {
            ProcessAnimationPuppet();
            return;
        }
        
        ProcessAnimation();
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
        if (IsServer)
        {
            uint currentTick = TimeManager.LocalTick;
            Server_SendAnimationState(new PlayerAnimationState{ AnimationVector = _animationVector, Tick = currentTick});
        }
    }

    public void CacheInput(NetworkInput input, float acceleration, float deceleration)
    {
        _cachedInput = input;
        _cachedAcceleration = acceleration;
        _cachedDeceleration = deceleration;
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

    private void ProcessAnimation()
    {
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
        _basicGroundState.Parameter = _animationVector;
    }

    private void ProcessAnimationPuppet()
    {
        _lerpValue += Time.deltaTime * _interpolationSpeed;
        _puppetAnimationVector = Vector2.Lerp(_puppetAnimationVector, _currentState.AnimationVector,
            _lerpValue);
        _basicGroundState.Parameter = _puppetAnimationVector;
    }

    private void GrabAnimationStates()
    {
        while (_puppetAnimationStates.Count > 1)
        {
            _currentState = _puppetAnimationStates.Read();
            _lerpValue = 0;
        }
    }
}
