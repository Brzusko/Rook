using System.Collections;
using System.Collections.Generic;
using IT.Data.Networking;
using IT.Interfaces.FSM;
using UnityEngine;

namespace IT.FSM.States
{
    public class PrepareSwingCombatIcspState : ICSPState<NetworkInput>
    {
        private readonly ICSPStateMachine<PlayerBaseStateID, PlayerCombatStateID, PlayerStateMachineContext> _icspStateMachine;

        public PrepareSwingCombatIcspState(
            ICSPStateMachine<PlayerBaseStateID, PlayerCombatStateID, PlayerStateMachineContext> icspStateMachine)
        {
            _icspStateMachine = icspStateMachine;
        }
        
        public void Tick(NetworkInput input, bool asServer, bool isReplaying, float deltaTime)
        {
            PlayerStateMachineContext context = _icspStateMachine.Context;
            CombatModule combatModule = context.CombatModule;

            context.CurrentPrepareSwingTime = Mathf.MoveTowards(context.CurrentPrepareSwingTime,
                combatModule.PrepareTimeInSeconds, deltaTime);
            
        }

        public void Enter(bool onReconcile, bool asServer, bool asReplay = false)
        {
            MovementStatsModule movementStatsModule = _icspStateMachine.Context.MovementStatsModule;
            movementStatsModule.SetAdditionalSpeedModifiers(movementStatsModule.PrepareSwingModifier);
            
            _icspStateMachine.Context.PlayerAnimations.PlayAnimation(PlayerCombatAnimID.PREPARE_SWING);
            
            if(onReconcile)
                return;
            
            _icspStateMachine.Context.CurrentPrepareSwingTime = 0f;
        }

        public void Exit(bool onReconcile, bool asReplay = false)
        {
            MovementStatsModule movementStatsModule = _icspStateMachine.Context.MovementStatsModule;
            movementStatsModule.ResetAdditionalSpeedModifiers();
        }

        public void CheckStateChange(NetworkInput input, bool onReconcile, bool asServer, bool asReplay = false)
        {
            PlayerBaseStateID baseStateID = _icspStateMachine.BaseStateID;
            
            if (baseStateID is PlayerBaseStateID.JUMPING or PlayerBaseStateID.FALLING)
            {
                _icspStateMachine.ChangeSecondaryState(PlayerCombatStateID.IDLE, onReconcile, asServer, asReplay);
                return;
            }

            if(input.IsMainActionPressed)
                return;
            
            PlayerStateMachineContext context = _icspStateMachine.Context;

            if (context.CurrentPrepareSwingTime < context.CombatModule.PrepareTimeInSeconds)
            {
                _icspStateMachine.ChangeSecondaryState(PlayerCombatStateID.IDLE, onReconcile, asServer, asReplay);
                return;
            }
            
            _icspStateMachine.ChangeSecondaryState(PlayerCombatStateID.SWING, onReconcile, asServer, asReplay);
        }
    }
}
