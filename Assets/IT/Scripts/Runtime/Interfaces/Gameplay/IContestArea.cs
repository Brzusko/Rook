using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Interfaces
{
    public interface IContestArea
    {
        public event Action<IPointCounter> PointCounterGainAllPoints;
        public void Restart();
    }
}
