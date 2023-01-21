using System.Collections;
using System.Collections.Generic;
using IT.FSM;
using IT.Interfaces.FSM;
using UnityEngine;
using EasyCharacterMovement;
using IT.Data.Networking;

public class PlayerScutterState : IState<NetworkInput>
{
    private readonly IStateMachine<PlayerStateID, MovementContext> _stateMachine;
    public PlayerScutterState(IStateMachine<PlayerStateID, MovementContext> stateMachine)
    {
        _stateMachine = stateMachine;
    }
    
    public void Tick(NetworkInput input, bool asServer, bool isReplaying, float deltaTime)
    {
        MovementContext context = _stateMachine.Context;
        MovementStatsModule movementStatsModule = context.MovementStatsModule;
        CharacterMovement characterMovement = context.CharacterMovement;

        context.Rotator.RotateY(input.YRotation, movementStatsModule.RotationSpeed, deltaTime);

        float maxSpeed = movementStatsModule.MovementSpeed;

        Vector3 desiredVelocity = new Vector3(input.MovementInput.x, 0f, input.MovementInput.y) * maxSpeed;
                                      
        characterMovement.SimpleMove(desiredVelocity, 
            maxSpeed, 
            movementStatsModule.Acceleration, 
            movementStatsModule.Deceleration, 
            movementStatsModule.Friction, 
            movementStatsModule.Drag, 
            new Vector3(0f, movementStatsModule.Gravity, 0f), 
            true, 
            deltaTime);
    }

    public void Enter()
    {
        
    }

    public void Exit()
    {
        
    }

    public void CheckStateChange(NetworkInput input)
    {
        if (input.IsWalkingPressed && input.MovementInput.sqrMagnitude > 0f)
        {
            _stateMachine.ChangeState(PlayerStateID.WALKING);
            return;
        }
            
        if (input.MovementInput.sqrMagnitude == 0f)
        {
            _stateMachine.ChangeState(PlayerStateID.IDLE);
        }
    }
}
