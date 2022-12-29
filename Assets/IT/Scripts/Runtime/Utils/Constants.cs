using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Utils
{
    public static class Constants
    {
        public static readonly byte NULL = 0;
        public static readonly byte FORWARD_FLAG = 1 << 1;
        public static readonly byte BACKWARD_FLAG = 1 << 2;
        public static readonly byte LEFT_FLAG = 1 << 3;
        public static readonly byte RIGHT_FLAG = 1 << 4;
        public static readonly float GRAVITY = 9.81f;
        public static readonly float FRICTION = 1.0f;
        public static readonly float AIR_CONTROL = 1.0f;
    }
}
