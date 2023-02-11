using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Interfaces.FSM
{
    public interface ICSPState<TInput>
    {
        public void Tick(TInput input, bool asServer, bool isReplaying, float deltaTime);
        public void Enter(bool onReconcile, bool asServer, bool asReplay = false);
        public void Exit(bool onReconcile, bool asReplay = false);

        public void CheckStateChange(TInput input, bool onReconcile, bool asServer, bool asReplay = false);
    }
}
