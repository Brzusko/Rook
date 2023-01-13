using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using UnityEngine;

namespace IT.Interfaces
{
    public interface IPlayersConsciousness
    {
        public IPlayerConsciousness CreatePlayerConsciousness(NetworkConnection connection);
    }
}
