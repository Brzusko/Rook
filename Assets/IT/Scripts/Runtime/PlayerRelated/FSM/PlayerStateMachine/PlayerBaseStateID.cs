using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.FSM
{
    public enum PlayerBaseStateID : byte
    {
        IDLE,
        WALKING,
        SCUTTER,
        FALLING,
        JUMPING,
    }

    public enum PlayerSecondaryStateID : byte
    {
        
    }
}
