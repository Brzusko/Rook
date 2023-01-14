using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Interfaces
{
    public interface IPlayerConsciousness : INetworkObject
    {
        public bool HasPossession { get; }
        public void Initialize(IPlayersConsciousness playersConsciousness);
        public void Possess(IEntityToPossess entityToPossess);
        public void RevokeCurrentPossession(bool clearCache = false);
    }
}
