using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using UnityEngine;

namespace IT.Interfaces
{
    public interface IPlayerSpawner<T>
    {
        public void SpawnPlayers(IEnumerable<T> connections);
        public void UnclaimSpawnpoints();
    }
}
