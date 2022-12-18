using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Utils.PrimitiveExtensions
{
    public static class FloatExtensions
    {
        public static float ClampToMax(this float value, float max)
        {
            return value <= max ? value : max;
        }
    }
}
