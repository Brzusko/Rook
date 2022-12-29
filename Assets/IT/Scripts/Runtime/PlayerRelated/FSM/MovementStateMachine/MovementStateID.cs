using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.FSM
{
    public enum MovementStateID : byte
    {
        IDLE,
        MOVING,
        JUMPING,
        AIR,
    }
}
