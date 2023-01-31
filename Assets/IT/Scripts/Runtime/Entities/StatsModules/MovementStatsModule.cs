using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using IT.Stats;
using IT.Utils;
using UnityEngine;

public class MovementStatsModule : NetworkBehaviour
{
    [Header("Grounded Movement Stats")]
    [SerializeField] private float _movementSpeed = 1f;
    [SerializeField] private float _walkingSpeedModifier = 0.75f;
    [SerializeField]private float _additionalSpeedModifiers = 1f;
    [SerializeField] private float _jumpForce = 3f;
    [SerializeField] private float _acceleration = 1f;
    [SerializeField] private float _deceleration = 1f;
    [SerializeField] private float _friction = 1f;
    [SerializeField] private float _drag = 1f;
    [SerializeField] private float _gravity = 1f;
    [Header("In Air Movement Stats")]
    [SerializeField] private float _inAirAcceleration = 1f;
    [SerializeField] private float _inAirDeceleration = 1f;
    [SerializeField] private float _inAirFriction = 1f;
    [SerializeField] private float _inAirDrag = 1f;
    [SerializeField] private float _inAirControl = 1f;

    public float MovementSpeed => _movementSpeed * _additionalSpeedModifiers;
    public float RotationSpeed => Constants.MAX_ROTATION;
    public float WalkingSpeedModifier => _walkingSpeedModifier;
    public float JumpForce => _jumpForce;
    public float AdditionalSpeedModifiers => _additionalSpeedModifiers;
    public float Acceleration => _acceleration;
    public float Deceleration => _deceleration;
    public float Friction => _friction;
    public float Drag => _drag;
    public float Gravity => - _gravity;
    public float InAirAcceleration => _inAirAcceleration;
    public float InAirDeceleration => _inAirDeceleration;
    public float InAirDrag => _inAirDrag;
    public float InAirFriction => _inAirFriction;
    public float InAirControl => _inAirControl;

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        InitializeOnce();
    }
    
    private void InitializeOnce()
    {
    }

    public void ResetAdditionalSpeedModifiers() => _additionalSpeedModifiers = 1f;
    public void SetAdditionalSpeedModifiers(float value) => _additionalSpeedModifiers = value;
}
