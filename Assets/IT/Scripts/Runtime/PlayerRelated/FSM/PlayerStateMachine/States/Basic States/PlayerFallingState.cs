using System.Collections;
using System.Collections.Generic;
using EasyCharacterMovement;
using IT.Data.Networking;
using IT.FSM;
using IT.Interfaces.FSM;
using UnityEngine;

public class PlayerFallingState : IState<NetworkInput>
{
    private readonly IStateMachine<PlayerStateID, MovementContext> _stateMachine;
    private float _inTheAir = 0f;
    
    public PlayerFallingState(IStateMachine<PlayerStateID, MovementContext> stateMachine)
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

        int layerMask = ~0;

        if (characterMovement.Raycast(characterMovement.GetFootPosition(), Vector3.down, 1.5f, layerMask,
                out RaycastHit hit, 0.1f) && _inTheAir > 0.1f)
        {
            context.PlayerAnimations.PlayAnimation(PlayerAnimationStateID.LANDING);
        }
        
        characterMovement.SimpleMove(desiredVelocity, 
            maxSpeed, 
            movementStatsModule.InAirAcceleration, 
            movementStatsModule.Deceleration, 
            movementStatsModule.InAirFriction, 
            movementStatsModule.Drag, 
            new Vector3(0f, movementStatsModule.Gravity, 0f), 
            true, 
            deltaTime);

        _inTheAir += deltaTime;
    }

    public void Enter()
    {
        _stateMachine.Context.PlayerAnimations.PlayAnimation(PlayerAnimationStateID.FALLING);
        _inTheAir = 0f;
    }

    public void Exit()
    {
        
    }

    public void CheckStateChange(NetworkInput input)
    {
        CharacterMovement characterMovement = _stateMachine.Context.CharacterMovement;

        if (characterMovement.wasGrounded || !characterMovement.isGrounded)
            return;

        if (input.MovementInput.sqrMagnitude == 0)
        {
            _stateMachine.ChangeState(PlayerStateID.IDLE);
            return;
        }

        if (input.IsWalkingPressed)
        {
            _stateMachine.ChangeState(PlayerStateID.WALKING);
            return;
        }
        
        _stateMachine.ChangeState(PlayerStateID.SCUTTER);
    }
}
