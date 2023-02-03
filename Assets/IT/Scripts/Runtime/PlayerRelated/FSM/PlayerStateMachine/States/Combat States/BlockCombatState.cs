using System.Collections;
using System.Collections.Generic;
using IT.Data.Networking;
using IT.Interfaces.FSM;
using UnityEngine;

namespace IT.FSM.States
{
    public class BlockCombatState : IState<NetworkInput>
    {
        private readonly IStateMachine<PlayerBaseStateID, PlayerCombatStateID, PlayerStateMachineContext> _stateMachine;

        public BlockCombatState(
            IStateMachine<PlayerBaseStateID, PlayerCombatStateID, PlayerStateMachineContext> stateMachine)
        {
            _stateMachine = stateMachine;
        }
        
        public void Tick(NetworkInput input, bool asServer, bool isReplaying, float deltaTime)
        {
            
        }

        public void Enter(bool onReconcile, bool asReplay = false)
        {
            MovementStatsModule movementStatsModule = _stateMachine.Context.MovementStatsModule;
            movementStatsModule.SetAdditionalSpeedModifiers(movementStatsModule.BlockModifier);
            
            if(asReplay)
                return;
            
            _stateMachine.Context.PlayerAnimations.PlayAnimation(PlayerCombatAnimID.BLOCK, 0.1f);
        }

        public void Exit(bool onReconcile, bool asReplay = false)
        {
            MovementStatsModule movementStatsModule = _stateMachine.Context.MovementStatsModule;
            movementStatsModule.ResetAdditionalSpeedModifiers();
        }

        public void CheckStateChange(NetworkInput input, bool onReconcile, bool asReplay = false)
        {
            PlayerBaseStateID baseStateID = _stateMachine.BaseStateID;

            if (baseStateID is PlayerBaseStateID.JUMPING or PlayerBaseStateID.FALLING || !input.IsSecondaryActionPressed)
            {
                _stateMachine.ChangeSecondaryState(PlayerCombatStateID.IDLE, onReconcile, asReplay);
                return;
            }
        }
    }
}
