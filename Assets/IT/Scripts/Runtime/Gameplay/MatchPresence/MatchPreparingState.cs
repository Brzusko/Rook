using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using IT.Interfaces.FSM;
using UnityEngine;

namespace IT.Gameplay
{
    public class MatchPreparingState : NetworkBehaviour, IState<MatchStatesID>
    {
        public MatchStatesID StateID => MatchStatesID.PREPARING;

        public void Enter(bool asServer)
        {
            
        }

        public void Exit(bool asServer)
        {
            
        }
    }
}
