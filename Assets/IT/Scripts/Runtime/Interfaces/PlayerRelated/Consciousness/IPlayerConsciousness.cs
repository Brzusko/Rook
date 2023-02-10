using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Interfaces
{
    public interface IPlayerConsciousness : INetworkObject
    {
        public bool HasPossession { get; }
        public void BindEntity(IEntityToPossess entityToPossess);
        public void Possess();
        public void Unpossess();
    }
}
