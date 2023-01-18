using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;
using Animancer;
using EasyCharacterMovement;
using UnityEditor.Animations;

public class PlayerAnimations : NetworkBehaviour
{
    [Header("Components")]
    [SerializeField] private AnimancerComponent _animancerComponent;
    [SerializeField] private CharacterMovement _characterMovement;
    [SerializeField] private Transform _playerMainSpaceTransform;
    [Header("Animation Assets")]
    [SerializeField] private MixerTransition2DAsset.UnShared _basicGroundMixer;
    [Header("General")]
    [Range(1f, 10f)]
    [SerializeField] private float _maxAnimationVectorMagnitude = 2f;

    private MixerState<Vector2> _basicGroundState;
    private bool _areEventsBound;

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
        if(!IsOwner)
            return;

        if(!_basicGroundState.IsPlaying)
            return;
        
        Vector3 clampedVelocity = Vector3.ClampMagnitude(_characterMovement.velocity, _maxAnimationVectorMagnitude);
        clampedVelocity = _playerMainSpaceTransform.InverseTransformDirection(clampedVelocity);
        Vector2 newStateParam = new Vector2(clampedVelocity.x, clampedVelocity.z);

        _basicGroundState.Parameter = newStateParam;
    }
}
