using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Interfaces.FSM
{
    public interface IState<TInput>
    {
        public void Tick(TInput input, bool asServer, bool isReplaying, float deltaTime);
        public void Enter();
        public void Exit();

        public void CheckStateChange();
    }
}
