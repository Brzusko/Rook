using System.Collections;
using System.Collections.Generic;
using IT.Data.Networking;
using IT.Interfaces.FSM;
using UnityEngine;

namespace IT.FSM.States
{
    public class IdleCombatIcspState : ICSPState<NetworkInput>
    {
        private readonly ICSPStateMachine<PlayerBaseStateID, PlayerCombatStateID, PlayerStateMachineContext> _icspStateMachine;

        public IdleCombatIcspState(
            ICSPStateMachine<PlayerBaseStateID, PlayerCombatStateID, PlayerStateMachineContext> icspStateMachine)
        {
            _icspStateMachine = icspStateMachine;
        }
        
        public void Tick(NetworkInput input, bool asServer, bool isReplaying, float deltaTime)
        {
            
        }

        public void Enter(bool onReconcile, bool asReplay = false)
        {
            _icspStateMachine.Context.PlayerAnimations.PlayAnimation(PlayerCombatAnimID.NONE);
        }

        public void Exit(bool onReconcile, bool asReplay = false)
        {
            
        }

        public void CheckStateChange(NetworkInput input, bool onReconcile, bool asReplay = false)
        {
            PlayerBaseStateID baseStateID = _icspStateMachine.BaseStateID;
            
            if(baseStateID is PlayerBaseStateID.JUMPING or PlayerBaseStateID.FALLING)
                return;

            if (input.IsSecondaryActionPressed)
            {
                _icspStateMachine.ChangeSecondaryState(PlayerCombatStateID.BLOCK, onReconcile, asReplay);
                return;
            }
            
            if(!input.IsMainActionPressed)
                return;
            
            _icspStateMachine.ChangeSecondaryState(PlayerCombatStateID.PREPARE_SWING, onReconcile, asReplay);
        }
    }
}
