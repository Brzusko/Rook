using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Interfaces.FSM
{
    public interface IStateMachine<in TID, in TID2, out TContext>
    {
        public TContext Context { get; }
        public void ChangeBaseState(TID stateID);
        public void ChangeSecondaryState(TID2 stateID);
    }
}
