using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using UnityEngine;

namespace IT.Interfaces
{
    public interface IPlayerSpawner
    {
        public void SpawnPlayers(IEnumerable<NetworkConnection> connections);
        public void UnclaimSpawnpoints();
    }
}
