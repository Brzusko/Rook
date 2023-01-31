using EasyCharacterMovement;
using IT.Interfaces;

namespace IT.FSM
{
    public class PlayerStateMachineContext
    {
        public PlayerStateMachineContext(CharacterMovement characterMovement, MovementStatsModule movementStatsModule, TransformRotator rotator, PlayerAnimations playerAnimations, IRaycaster raycaster)
        {
            CharacterMovement = characterMovement;
            MovementStatsModule = movementStatsModule;
            Rotator = rotator;
            PlayerAnimations = playerAnimations;
        }
        
        public CharacterMovement CharacterMovement { get; }
        public MovementStatsModule MovementStatsModule { get; }
        public TransformRotator Rotator { get; }
        public PlayerAnimations PlayerAnimations { get; }
    }
}
