using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Interfaces
{
    public interface IPlayerConsciousness : INetworkObject
    {
        public void Initialize(IPlayersConsciousness playersConsciousness);
        public void Possess(IEntityToPossess entityToPossess);
        public void RevokeCurrentPossession(bool clearCache = false);
    }
}
