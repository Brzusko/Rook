using System.Linq;
using System.Numerics;
using EasyCharacterMovement;
using IT.Data.Networking;
using IT.Input;
using IT.Interfaces;
using IT.Interfaces.FSM;
using IT.Utils;
using Vector3 = UnityEngine.Vector3;

namespace IT.FSM.States
{
    public class PlayerIdleState: IState<NetworkInput>
    {
        private readonly IStateMachine<PlayerStateID, MovementContext> _stateMachine;

        public PlayerIdleState(IStateMachine<PlayerStateID, MovementContext> stateMachine)
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

        public void Enter()
        {
            
        }

        public void Exit()
        {
            
        }

        public void CheckStateChange(NetworkInput input)
        {
            CharacterMovement characterMovement = _stateMachine.Context.CharacterMovement;

            if (input.MovementInput.sqrMagnitude == 0 && characterMovement.isGrounded)
                return;

            if (input.IsWalkingPressed)
            {
                _stateMachine.ChangeState(PlayerStateID.WALKING);
                return;
            }
            
            _stateMachine.ChangeState(PlayerStateID.SCUTTER);
        }
    }
}