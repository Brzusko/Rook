using System.Collections;
using System.Collections.Generic;
using EasyCharacterMovement;
using IT.Data.Networking;
using IT.FSM;
using IT.Interfaces.FSM;
using UnityEngine;

namespace IT.FSM.States
{
    public class MovementFallingIcspState : ICSPState<NetworkInput>
    {
        private readonly ICSPStateMachine<PlayerBaseStateID, PlayerCombatStateID, PlayerStateMachineContext> _icspStateMachine;
        private float _inTheAir = 0f;
        
        public MovementFallingIcspState(ICSPStateMachine<PlayerBaseStateID, PlayerCombatStateID, PlayerStateMachineContext> icspStateMachine)
        {
            _icspStateMachine = icspStateMachine;
        }
        
        public void Tick(NetworkInput input, bool asServer, bool isReplaying, float deltaTime)
        {
            PlayerStateMachineContext context = _icspStateMachine.Context;
            MovementStatsModule movementStatsModule = context.MovementStatsModule;
            CharacterMovement characterMovement = context.CharacterMovement;
                
            context.Rotator.RotateY(input.YRotation, movementStatsModule.RotationSpeed, deltaTime);

            float maxSpeed = movementStatsModule.MovementSpeed *  movementStatsModule.InAirControl;
            
            float acceleration = characterMovement.isGrounded
                ? movementStatsModule.Acceleration 
                : movementStatsModule.InAirAcceleration;
            
            float deceleration = characterMovement.isGrounded
                ? movementStatsModule.Deceleration
                : movementStatsModule.InAirDeceleration;
            
            float friction = characterMovement.isGrounded
                ? movementStatsModule.Friction
                : movementStatsModule.InAirFriction;
            
            float drag = characterMovement.isGrounded
                ? movementStatsModule.Drag
                : movementStatsModule.InAirDrag;

            Vector3 desiredVelocity = new Vector3(input.MovementInput.x, 0f, input.MovementInput.y) * maxSpeed;

            int layerMask = ~0;

            if (characterMovement.Raycast(characterMovement.GetFootPosition(), Vector3.down, 1f, layerMask,
                    out RaycastHit hit, 0.1f) && _inTheAir > 0.05f && !isReplaying)
            {
                context.PlayerAnimations.PlayAnimation(PlayerMovementAnimID.LANDING);
            }
            
            characterMovement.SimpleMove(desiredVelocity,
                maxSpeed,
                acceleration,
                deceleration,
                friction,
                drag,
                new Vector3(0f, movementStatsModule.Gravity, 0f),
                true,
                deltaTime);

            _inTheAir += deltaTime;
        }

        public void Enter(bool onReconcile, bool asServer, bool asReplay = false)
        {
            if(asReplay)
                return;

            _icspStateMachine.Context.PlayerAnimations.PlayAnimation(PlayerMovementAnimID.FALLING);
            _inTheAir = 0f;
        }

        public void Exit(bool onReconcile, bool asReplay = false)
        {
            
        }

        public void CheckStateChange(NetworkInput input, bool onReconcile, bool asServer, bool asReplay = false)
        {
            CharacterMovement characterMovement = _icspStateMachine.Context.CharacterMovement;

            if (characterMovement.wasGrounded || !characterMovement.isGrounded)
                return;

            if (input.MovementInput.sqrMagnitude == 0)
            {
                _icspStateMachine.ChangeBaseState(PlayerBaseStateID.IDLE, onReconcile, asServer, asReplay);
                return;
            }

            if (input.IsWalkingPressed)
            {
                _icspStateMachine.ChangeBaseState(PlayerBaseStateID.WALKING, onReconcile, asServer, asReplay);
                return;
            }
            
            _icspStateMachine.ChangeBaseState(PlayerBaseStateID.SCUTTER,  onReconcile, asServer, asReplay);
        }
    }

}