using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Interfaces
{
    public interface ISpawnPoint
    {
        public Vector3 SpawnPoint { get; }
        public bool IsOccupied { get; }

        public void Claim();
        public void Unclaim();
    }
}
