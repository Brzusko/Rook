using System.Collections;
using System.Collections.Generic;
using FishNet.Component.ColliderRollback;
using EasyCharacterMovement;
using FishNet.Managing.Timing;
using FishNet.Object;
using IT.FSM;
using UnityEngine;

public class CombatModule : NetworkBehaviour
{
    [SerializeField] private float _prepareTimeInSeconds = 1f;
    [SerializeField] private float _swingTimeInSeconds = 1f;
    [SerializeField] private float _forwardPushScalar;
    [SerializeField] private float _upPushScalar;
    [SerializeField] private PlayerStateMachine _playerStateMachine;

    public float PrepareTimeInSeconds
    {
        get => _prepareTimeInSeconds;
        set => _prepareTimeInSeconds = value;
    }

    public float SwingTimeInSeconds
    {
        get => _swingTimeInSeconds;
        set => _swingTimeInSeconds = value;
    }

    private void LaunchCharacter(Vector3 hitNormal)
    {
        Vector3 hitDirection = -new Vector3(hitNormal.x, 0f, hitNormal.z);
        Vector3 upForce = Vector3.up * _upPushScalar;
        Vector3 forwardForce = hitDirection * _forwardPushScalar;
        Vector3 hitForce = forwardForce + upForce;

        uint currentTick = TimeManager.Tick;
        double currentTime = TimeManager.TicksToTime(currentTick);
        double currentTimePlusSwingTime = currentTime + _swingTimeInSeconds;
        uint tickToComplete = TimeManager.TimeToTicks(currentTimePlusSwingTime);
        uint tickDelta = tickToComplete - currentTick;
        
        Debug.Log($"Tick {currentTick}, Tick plus swing {tickToComplete}, tick delta {tickDelta}");
        
        _playerStateMachine.CacheKnockback(hitForce, tickDelta);
    }
    
    public void RequestHit()
    {
        PreciseTick preciseTick = TimeManager.GetPreciseTick(TickType.LastPacketTick);
        ServerHit(preciseTick);
    }
    
    [ServerRpc]
    private void ServerHit(PreciseTick preciseTick)
    {
        RollbackManager.Rollback(preciseTick, RollbackManager.PhysicsType.ThreeDimensional, IsOwner);
        int mask = 1 << LayerMask.NameToLayer("HitBox");

        Transform t = transform;
        
        Debug.Log($"Sec {_swingTimeInSeconds}, Tick time: ${TimeManager.TicksToTime(TimeManager.Tick)}, Tick time plus swing {TimeManager.TicksToTime(TimeManager.Tick) + (_swingTimeInSeconds)},Tick {TimeManager.Tick} , Calc tick {TimeManager.TimeToTicks(TimeManager.TicksToTime(TimeManager.Tick) + (_swingTimeInSeconds))}");

        if(Physics.Raycast(t.position, t.forward, out RaycastHit hit, 1f, mask))
        { // add block rollback
            if (hit.collider.TryGetComponent(out CombatModule combatModule))
            {
                combatModule.LaunchCharacter(hit.normal);
            }
        }
        
        RollbackManager.Return();
    }
}
