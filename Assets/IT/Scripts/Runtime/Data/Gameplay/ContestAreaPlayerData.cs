using System.Collections;
using System.Collections.Generic;
using IT.Interfaces;
using UnityEngine;

namespace IT.Data.Gameplay
{
    public class ContestAreaPlayerData
    {
        public IPointCounter PointCounter;
        public bool WasContestingPastTick;
    }
}
