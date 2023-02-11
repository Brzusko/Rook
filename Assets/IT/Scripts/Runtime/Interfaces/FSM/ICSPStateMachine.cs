using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Interfaces.FSM
{
    public interface ICSPStateMachine<TID, TID2, out TContext>
    {
        public TContext Context { get; }
        public TID BaseStateID { get; }
        public TID2 SecondaryStateID { get; }
        public void ChangeBaseState(TID stateID, bool onReconcile, bool asServer, bool asReplay = false);
        public void ChangeSecondaryState(TID2 stateID, bool onReconcile, bool asServer, bool asReplay = false);
    }
}
