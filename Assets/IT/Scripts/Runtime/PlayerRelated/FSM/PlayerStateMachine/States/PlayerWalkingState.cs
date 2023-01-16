using EasyCharacterMovement;
using IT.Input;
using IT.Interfaces.FSM;
using IT.Utils;
using UnityEngine;

namespace IT.FSM.States
{
    public class PlayerWalkingState: IState<NetworkedInput>
    {
        private IStateMachine<PlayerStateID, MovementContext> _stateMachine;

        public PlayerWalkingState(IStateMachine<PlayerStateID, MovementContext> stateMachine)
        {
            _stateMachine = stateMachine;
        }
        public void Tick(NetworkedInput input, bool asServer, bool isReplaying, float deltaTime)
        {
            MovementContext context = _stateMachine.Context;
            MovementStatsModule movementStatsModule = context.MovementStatsModule;
            
            if (!asServer)
            {
                
            }

            float maxSpeed = movementStatsModule.MovementSpeed;

            Vector3 desiredVelocity = new Vector3(input.MovementInput.x, 0f, input.MovementInput.y) * maxSpeed;
                                      
            context.CharacterMovement.SimpleMove(desiredVelocity, 
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

        public void CheckStateChange()
        {
            CharacterMovement characterMovement = _stateMachine.Context.CharacterMovement;

            if (characterMovement.velocity.sqrMagnitude == 0f)
            {
                _stateMachine.ChangeState(PlayerStateID.IDLE);
            }
        }
    }
}