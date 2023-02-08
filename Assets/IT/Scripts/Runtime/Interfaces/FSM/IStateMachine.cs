using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Interfaces.FSM
{
    public interface IStateMachine<TID>
    {
        public void ChangeState(TID stateID, bool asServer);
    }
}
