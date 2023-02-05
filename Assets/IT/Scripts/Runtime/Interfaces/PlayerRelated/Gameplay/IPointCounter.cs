using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Interfaces
{
    public interface IPointCounter : INetworkObject
    {
        public uint CurrentPoints { get; }
        public void GainPoints();
        public void ResetCounter();
    }
}
