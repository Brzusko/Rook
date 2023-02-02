using System.Collections;
using System.Collections.Generic;
using IT.Data.Networking;
using IT.Interfaces.FSM;
using UnityEngine;

namespace IT.FSM.States
{
    public class SwingCombatState : IState<NetworkInput>
    {
        private readonly IStateMachine<PlayerBaseStateID, PlayerCombatStateID, PlayerStateMachineContext> _stateMachine;

        public SwingCombatState(
            IStateMachine<PlayerBaseStateID, PlayerCombatStateID, PlayerStateMachineContext> stateMachine)
        {
            _stateMachine = stateMachine;
        }
        
        public void Tick(NetworkInput input, bool asServer, bool isReplaying, float deltaTime)
        {
            PlayerStateMachineContext context = _stateMachine.Context;
            CombatModule combatModule = context.CombatModule;

            context.CurrentSwingTime = Mathf.MoveTowards(context.CurrentSwingTime,
                combatModule.SwingTimeInSeconds, deltaTime);

            if (context.CurrentSwingTime >= combatModule.SwingTimeInSeconds && !isReplaying && !asServer)
            {
                
            }
        }

        public void Enter(bool onReconcile, bool asReplay = false)
        {
            PlayerStateMachineContext context = _stateMachine.Context;
            CombatModule combatModule = context.CombatModule;
            MovementStatsModule movementStatsModule = _stateMachine.Context.MovementStatsModule;
            movementStatsModule.SetAdditionalSpeedModifiers(movementStatsModule.PrepareSwingModifier);
            
            if(onReconcile)
                return;
            
            combatModule.RequestHit();
            context.CurrentSwingTime = 0f;
        }

        public void Exit(bool onReconcile, bool asReplay = false)
        {
            MovementStatsModule movementStatsModule = _stateMachine.Context.MovementStatsModule;
            movementStatsModule.ResetAdditionalSpeedModifiers();
        }

        public void CheckStateChange(NetworkInput input, bool onReconcile, bool asReplay = false)
        {
            PlayerBaseStateID baseStateID = _stateMachine.BaseStateID;
            
            if (baseStateID is PlayerBaseStateID.JUMPING or PlayerBaseStateID.FALLING || _stateMachine.Context.CurrentSwingTime >= _stateMachine.Context.CombatModule.SwingTimeInSeconds)
            {
                _stateMachine.ChangeSecondaryState(PlayerCombatStateID.IDLE, onReconcile, asReplay);
                return;
            }

            if (!input.IsSecondaryActionPressed) return;
            _stateMachine.ChangeSecondaryState(PlayerCombatStateID.BLOCK, onReconcile, asReplay);
        }
    }
}
