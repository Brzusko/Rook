using System.Collections;
using System.Collections.Generic;
using IT.Data.Networking;
using IT.Interfaces.FSM;
using UnityEngine;

namespace IT.FSM.States
{
    public class BlockCombatIcspState : ICSPState<NetworkInput>
    {
        private readonly ICSPStateMachine<PlayerBaseStateID, PlayerCombatStateID, PlayerStateMachineContext> _icspStateMachine;

        public BlockCombatIcspState(
            ICSPStateMachine<PlayerBaseStateID, PlayerCombatStateID, PlayerStateMachineContext> icspStateMachine)
        {
            _icspStateMachine = icspStateMachine;
        }
        
        public void Tick(NetworkInput input, bool asServer, bool isReplaying, float deltaTime)
        {
            
        }

        public void Enter(bool onReconcile, bool asServer, bool asReplay = false)
        {
            MovementStatsModule movementStatsModule = _icspStateMachine.Context.MovementStatsModule;
            movementStatsModule.SetAdditionalSpeedModifiers(movementStatsModule.BlockModifier);
            
            if(asReplay)
                return;
            
            _icspStateMachine.Context.PlayerAnimations.PlayAnimation(PlayerCombatAnimID.BLOCK);
        }

        public void Exit(bool onReconcile, bool asReplay = false)
        {
            MovementStatsModule movementStatsModule = _icspStateMachine.Context.MovementStatsModule;
            movementStatsModule.ResetAdditionalSpeedModifiers();
        }

        public void CheckStateChange(NetworkInput input, bool onReconcile, bool asServer, bool asReplay = false)
        {
            PlayerBaseStateID baseStateID = _icspStateMachine.BaseStateID;

            if (baseStateID is PlayerBaseStateID.JUMPING or PlayerBaseStateID.FALLING || !input.IsSecondaryActionPressed)
            {
                _icspStateMachine.ChangeSecondaryState(PlayerCombatStateID.IDLE,  onReconcile, asServer, asReplay);
                return;
            }
        }
    }
}
