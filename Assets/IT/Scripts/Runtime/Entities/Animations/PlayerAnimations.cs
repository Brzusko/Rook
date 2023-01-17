using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;
using Animancer;

public class PlayerAnimations : NetworkBehaviour
{
    [SerializeField] private AnimancerComponent _animancerComponent;
    [SerializeField] private MixerTransition2DAsset.UnShared _defaultMixer;
    [SerializeField] private Vector2 _debugVector;
    
    private MixerState<Vector2> _defaultState;

    private void Awake()
    {
        InitializeOnce();
    }

    private void InitializeOnce()
    {
        _defaultState = (MixerState<Vector2>)_animancerComponent.States.GetOrCreate(_defaultMixer);
        _animancerComponent.Playable.UnpauseGraph();
        _animancerComponent.Play(_defaultState);
    }

    private void Update()
    {
        _defaultState.Parameter = _debugVector;
    }
}
