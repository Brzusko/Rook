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
    }
}
