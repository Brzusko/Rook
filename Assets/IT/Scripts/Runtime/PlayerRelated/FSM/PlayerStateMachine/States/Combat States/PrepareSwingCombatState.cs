using System.Collections;
using System.Collections.Generic;
using IT.Data.Networking;
using IT.Interfaces.FSM;
using UnityEngine;

namespace IT.FSM.States
{
    public class PrepareSwingCombatState : IState<NetworkInput>
    {
        private readonly IStateMachine<PlayerBaseStateID, PlayerCombatStateID, PlayerStateMachineContext> _stateMachine;

        public PrepareSwingCombatState(
            IStateMachine<PlayerBaseStateID, PlayerCombatStateID, PlayerStateMachineContext> stateMachine)
        {
            _stateMachine = stateMachine;
        }
        
        public void Tick(NetworkInput input, bool asServer, bool isReplaying, float deltaTime)
        {
            PlayerStateMachineContext context = _stateMachine.Context;
            CombatModule combatModule = context.CombatModule;

            context.CurrentPrepareSwingTime = Mathf.MoveTowards(context.CurrentPrepareSwingTime,
                combatModule.PrepareTimeInSeconds, deltaTime);
        }

        public void Enter(bool onReconcile, bool asReplay = false)
        {
            MovementStatsModule movementStatsModule = _stateMachine.Context.MovementStatsModule;
            movementStatsModule.SetAdditionalSpeedModifiers(movementStatsModule.PrepareSwingModifier);
            
            _stateMachine.Context.PlayerAnimations.PlayAnimation(PlayerCombatAnimID.PREPARE_SWING);
            
            if(onReconcile)
                return;
            
            _stateMachine.Context.CurrentPrepareSwingTime = 0f;
        }

        public void Exit(bool onReconcile, bool asReplay = false)
        {
            MovementStatsModule movementStatsModule = _stateMachine.Context.MovementStatsModule;
            movementStatsModule.ResetAdditionalSpeedModifiers();
        }

        public void CheckStateChange(NetworkInput input, bool onReconcile, bool asReplay = false)
        {
            PlayerBaseStateID baseStateID = _stateMachine.BaseStateID;
            
            if (baseStateID is PlayerBaseStateID.JUMPING or PlayerBaseStateID.FALLING)
            {
                _stateMachine.ChangeSecondaryState(PlayerCombatStateID.IDLE, onReconcile, asReplay);
                return;
            }

            if(input.IsMainActionPressed)
                return;
            
            PlayerStateMachineContext context = _stateMachine.Context;

            if (context.CurrentPrepareSwingTime < context.CombatModule.PrepareTimeInSeconds)
            {
                _stateMachine.ChangeSecondaryState(PlayerCombatStateID.IDLE, onReconcile, asReplay);
                return;
            }
            
            _stateMachine.ChangeSecondaryState(PlayerCombatStateID.SWING, onReconcile, asReplay);
        }
    }
}
