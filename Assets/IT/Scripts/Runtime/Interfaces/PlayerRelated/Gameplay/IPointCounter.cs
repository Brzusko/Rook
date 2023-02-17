using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Interfaces
{
    public interface IPointCounter : INetworkObject
    {
        public int CurrentPoints { get; }
        public void GainPoints();
    }
}
