using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;
using Animancer;
using EasyCharacterMovement;
using IT.Data.Networking;
using UnityEditor.Animations;

public class PlayerAnimations : NetworkBehaviour
{
    [Header("Components")]
    [SerializeField] private AnimancerComponent _animancerComponent;
    [SerializeField] private Transform _playerMainSpaceTransform;
    [Header("Animation Assets")]
    [SerializeField] private MixerTransition2DAsset.UnShared _basicGroundMixer;

    private MixerState<Vector2> _basicGroundState;
    private bool _areEventsBound;

    private NetworkInput _cachedInput;
    private Vector2 _animationVector = Vector2.zero;
    private float _cachedAcceleration;
    private float _cachedDeceleration;
    private Vector2 _puppetAnimationVector;
    
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
        _areEventsBound = true;
    }

    private void UnbindEvents()
    {
        if(!_areEventsBound)
            return;

        base.TimeManager.OnUpdate -= OnUpdate;
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
            _basicGroundState.Parameter = _puppetAnimationVector;
            return;
        }
        
        Vector3 transformedVelocity =
            _playerMainSpaceTransform.InverseTransformDirection(new Vector3(_cachedInput.MovementInput.x, 0,
                _cachedInput.MovementInput.y));
        
        float animationScalar = _cachedInput.IsWalkingPressed ? 1 : 2;
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

        if (IsServer)
        {
            Server_SendAnimationState(new PlayerAnimationState{ AnimationVector = _animationVector });
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
        _puppetAnimationVector = animationState.AnimationVector;
    }
}
