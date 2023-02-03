using System.Collections;
using System.Collections.Generic;
using IT.Data.Networking;
using IT.Interfaces.FSM;
using UnityEngine;

namespace IT.FSM.States
{
    public class IdleCombatState : IState<NetworkInput>
    {
        private readonly IStateMachine<PlayerBaseStateID, PlayerCombatStateID, PlayerStateMachineContext> _stateMachine;

        public IdleCombatState(
            IStateMachine<PlayerBaseStateID, PlayerCombatStateID, PlayerStateMachineContext> stateMachine)
        {
            _stateMachine = stateMachine;
        }
        
        public void Tick(NetworkInput input, bool asServer, bool isReplaying, float deltaTime)
        {
            
        }

        public void Enter(bool onReconcile, bool asReplay = false)
        {
            if(asReplay)
                return;
            
            _stateMachine.Context.PlayerAnimations.PlayAnimation(PlayerCombatAnimID.NONE, 0.2f);
        }

        public void Exit(bool onReconcile, bool asReplay = false)
        {
            
        }

        public void CheckStateChange(NetworkInput input, bool onReconcile, bool asReplay = false)
        {
            PlayerBaseStateID baseStateID = _stateMachine.BaseStateID;
            
            if(baseStateID is PlayerBaseStateID.JUMPING or PlayerBaseStateID.FALLING)
                return;

            if (input.IsSecondaryActionPressed)
            {
                _stateMachine.ChangeSecondaryState(PlayerCombatStateID.BLOCK, onReconcile, asReplay);
                return;
            }
            
            if(!input.IsMainActionPressed)
                return;
            
            _stateMachine.ChangeSecondaryState(PlayerCombatStateID.PREPARE_SWING, onReconcile, asReplay);
        }
    }
}
