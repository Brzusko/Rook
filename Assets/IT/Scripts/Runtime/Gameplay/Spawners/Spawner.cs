using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

namespace IT.Gameplay
{
    public class Spawner : MonoBehaviour
    {
        [SerializeField] private NetworkObject _networkObject;

        public void SpawnPlayers(IEnumerable<NetworkConnection> connections)
        {
            
        }
    }
}
