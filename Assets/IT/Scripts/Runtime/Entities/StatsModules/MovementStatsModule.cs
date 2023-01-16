using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using IT.Stats;
using IT.Utils;
using UnityEngine;

public class MovementStatsModule : NetworkBehaviour
{
    private SingleStat _movementSpeed;
    private float _additionalSpeedModifiers = 1f;

    public float MovementSpeed => _movementSpeed.CurrentValue * _additionalSpeedModifiers;
    public float Acceleration => Constants.ACCELERATION;
    public float Deceleration => Constants.DECELERATION;
    public float Friction => Constants.FRICTION;
    public float Drag => Constants.DRAG;
    public float Gravity => - Constants.GRAVITY_FORCE;
    public float InAirAcceleration => Acceleration * Constants.AIR_CONTROL;
    public float AdditionalSpeedModifiers => _additionalSpeedModifiers;
    public float RotationSpeed => Constants.MAX_ROTATION;

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        InitializeOnce();
    }
    
    private void InitializeOnce()
    {
        _movementSpeed = new SingleStat(StatID.MOVEMENT_SPEED, 2.5f);
        _additionalSpeedModifiers = 1f;
    }

    public void ResetAdditionalSpeedModifiers() => _additionalSpeedModifiers = 1f;
    public void SetAdditionalSpeedModifiers(float value) => _additionalSpeedModifiers = value;
}
