using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Interfaces.FSM
{
    public interface IStateMachine<in TID, out TContext>
    {
        public TContext Context { get; }
        public void ChangeState(TID stateID);
    }
}
