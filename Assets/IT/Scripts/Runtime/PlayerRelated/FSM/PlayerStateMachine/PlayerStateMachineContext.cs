using EasyCharacterMovement;
using IT.Interfaces;
using UnityEngine;

namespace IT.FSM
{
    [System.Serializable]
    public class PlayerStateMachineContext
    {
        public PlayerStateMachineContext(
                CharacterMovement characterMovement, 
                MovementStatsModule movementStatsModule, 
                TransformRotator rotator, 
                PlayerAnimations playerAnimations, 
                IRaycaster raycaster,
                CombatModule combatModule
            )
        {
            CharacterMovement = characterMovement;
            MovementStatsModule = movementStatsModule;
            Rotator = rotator;
            PlayerAnimations = playerAnimations;
            Raycaster = raycaster;
            CombatModule = combatModule;
        }
        
        public CharacterMovement CharacterMovement { get; }
        public MovementStatsModule MovementStatsModule { get; }
        public TransformRotator Rotator { get; }
        public PlayerAnimations PlayerAnimations { get; }
        public CombatModule CombatModule { get; }
        public IRaycaster Raycaster { get; }
        public float CurrentPrepareSwingTime = 0f;
        public float CurrentSwingTime = 0f;
    }
    
    public struct KnockbackCache
    {
        public uint InvokeTime;
        public Vector3 KnockbackForce;
    }
}
