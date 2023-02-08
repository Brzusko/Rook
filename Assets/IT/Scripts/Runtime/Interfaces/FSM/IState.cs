using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Interfaces.FSM
{
    public interface IState<TID>
    {
        public TID StateID { get; }
        public void Enter(bool asServer);
        public void Exit(bool asServer);
    }
}
