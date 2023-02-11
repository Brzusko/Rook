using EasyCharacterMovement;
using IT.Data.Networking;
using IT.Interfaces.FSM;
using UnityEngine;

namespace IT.FSM.States
{
    public class MovementIdleIcspState: ICSPState<NetworkInput>
    {
        private readonly ICSPStateMachine<PlayerBaseStateID, PlayerCombatStateID, PlayerStateMachineContext> _icspStateMachine;

        public MovementIdleIcspState(ICSPStateMachine<PlayerBaseStateID, PlayerCombatStateID, PlayerStateMachineContext> icspStateMachine)
        {
            _icspStateMachine = icspStateMachine;
        }
        public void Tick(NetworkInput input, bool asServer, bool isReplaying, float deltaTime)
        {
            PlayerStateMachineContext context = _icspStateMachine.Context;
            MovementStatsModule movementStatsModule = context.MovementStatsModule;
            CharacterMovement characterMovement = context.CharacterMovement;
            
            context.Rotator.RotateY(input.YRotation, movementStatsModule.RotationSpeed, deltaTime);

            float maxSpeed = movementStatsModule.MovementSpeed;
            
            characterMovement.SimpleMove(Vector3.zero, 
                maxSpeed, 
                movementStatsModule.Acceleration, 
                movementStatsModule.Deceleration, 
                movementStatsModule.Friction, 
                movementStatsModule.Drag, 
                new Vector3(0f, movementStatsModule.Gravity, 0f), 
                true, 
                deltaTime);
        }

        public void Enter(bool onReconcile, bool asServer, bool asReplay = false)
        {
            if(asReplay)
                return;
            
            _icspStateMachine.Context.PlayerAnimations.PlayAnimation(PlayerMovementAnimID.GROUNDED);
        }

        public void Exit(bool onReconcile, bool asReplay = false)
        {
            
        }

        public void CheckStateChange(NetworkInput input, bool onReconcile, bool asServer, bool asReplay = false)
        {
            CharacterMovement characterMovement = _icspStateMachine.Context.CharacterMovement;

            if (characterMovement.isGrounded && input.IsJumpPressed)
            {
                _icspStateMachine.ChangeBaseState(PlayerBaseStateID.JUMPING,  onReconcile, asServer, asReplay);
                return;
            }
            
            if (!characterMovement.isGrounded)
            {
                _icspStateMachine.ChangeBaseState(PlayerBaseStateID.FALLING,  onReconcile, asServer, asReplay);
                return;
            }
            
            if (input.MovementInput.sqrMagnitude == 0 && characterMovement.isGrounded)
                return;

            _icspStateMachine.ChangeBaseState(PlayerBaseStateID.SCUTTER,  onReconcile, asServer, asReplay);
        }
    }
}