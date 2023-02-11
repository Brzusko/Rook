using System.Collections;
using System.Collections.Generic;
using IT.Data.Networking;
using IT.Interfaces.FSM;
using UnityEngine;

namespace IT.FSM.States
{
    public class SwingCombatIcspState : ICSPState<NetworkInput>
    {
        private readonly ICSPStateMachine<PlayerBaseStateID, PlayerCombatStateID, PlayerStateMachineContext> _icspStateMachine;

        public SwingCombatIcspState(
            ICSPStateMachine<PlayerBaseStateID, PlayerCombatStateID, PlayerStateMachineContext> icspStateMachine)
        {
            _icspStateMachine = icspStateMachine;
        }
        
        public void Tick(NetworkInput input, bool asServer, bool isReplaying, float deltaTime)
        {
            PlayerStateMachineContext context = _icspStateMachine.Context;
            CombatModule combatModule = context.CombatModule;

            context.CurrentSwingTime = Mathf.MoveTowards(context.CurrentSwingTime,
                combatModule.SwingTimeInSeconds, deltaTime);
        }

        public void Enter(bool onReconcile, bool asServer, bool asReplay = false)
        {
            PlayerStateMachineContext context = _icspStateMachine.Context;
            CombatModule combatModule = context.CombatModule;
            MovementStatsModule movementStatsModule = _icspStateMachine.Context.MovementStatsModule;
            movementStatsModule.SetAdditionalSpeedModifiers(movementStatsModule.PrepareSwingModifier);
            
            if(onReconcile || asReplay || asServer)
                return;
            
            combatModule.RequestHit();
            context.CurrentSwingTime = 0f;
            
            _icspStateMachine.Context.PlayerAnimations.PlayAnimation(PlayerCombatAnimID.SWING);
        }

        public void Exit(bool onReconcile, bool asReplay = false)
        {
            MovementStatsModule movementStatsModule = _icspStateMachine.Context.MovementStatsModule;
            movementStatsModule.ResetAdditionalSpeedModifiers();
        }

        public void CheckStateChange(NetworkInput input, bool onReconcile, bool asServer, bool asReplay = false)
        {
            PlayerBaseStateID baseStateID = _icspStateMachine.BaseStateID;
            
            if (baseStateID is PlayerBaseStateID.JUMPING or PlayerBaseStateID.FALLING || _icspStateMachine.Context.CurrentSwingTime >= _icspStateMachine.Context.CombatModule.SwingTimeInSeconds)
            {
                _icspStateMachine.ChangeSecondaryState(PlayerCombatStateID.IDLE, onReconcile, asServer, asReplay);
                return;
            }

            if (!input.IsSecondaryActionPressed) return;
            _icspStateMachine.ChangeSecondaryState(PlayerCombatStateID.BLOCK, onReconcile, asServer, asReplay);
        }
    }
}
