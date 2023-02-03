using EasyCharacterMovement;
using IT.Data.Networking;
using IT.Interfaces.FSM;
using UnityEngine;

namespace IT.FSM.States
{
    public class MovementIdleState: IState<NetworkInput>
    {
        private readonly IStateMachine<PlayerBaseStateID, PlayerCombatStateID, PlayerStateMachineContext> _stateMachine;

        public MovementIdleState(IStateMachine<PlayerBaseStateID, PlayerCombatStateID, PlayerStateMachineContext> stateMachine)
        {
            _stateMachine = stateMachine;
        }
        public void Tick(NetworkInput input, bool asServer, bool isReplaying, float deltaTime)
        {
            PlayerStateMachineContext context = _stateMachine.Context;
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

        public void Enter(bool onReconcile, bool asReplay = false)
        {
            if(asReplay)
                return;
            
            _stateMachine.Context.PlayerAnimations.PlayAnimation(PlayerMovementAnimID.GROUNDED, 0.3f);
        }

        public void Exit(bool onReconcile, bool asReplay = false)
        {
            
        }

        public void CheckStateChange(NetworkInput input, bool onReconcile, bool asReplay = false)
        {
            CharacterMovement characterMovement = _stateMachine.Context.CharacterMovement;

            if (characterMovement.isGrounded && input.IsJumpPressed)
            {
                _stateMachine.ChangeBaseState(PlayerBaseStateID.JUMPING, onReconcile, asReplay);
                return;
            }
            
            if (!characterMovement.isGrounded)
            {
                _stateMachine.ChangeBaseState(PlayerBaseStateID.FALLING, onReconcile, asReplay);
                return;
            }
            
            if (input.MovementInput.sqrMagnitude == 0 && characterMovement.isGrounded)
                return;

            _stateMachine.ChangeBaseState(PlayerBaseStateID.SCUTTER, onReconcile, asReplay);
        }
    }
}