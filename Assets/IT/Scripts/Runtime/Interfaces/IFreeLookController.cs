using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Interfaces
{
    public interface IFreeLookController : ICameraTargets
    {
        public void ChangeState(bool state);
    }
}
