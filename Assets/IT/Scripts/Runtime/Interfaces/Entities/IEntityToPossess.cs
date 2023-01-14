using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Interfaces
{
    public interface IEntityToPossess : INetworkObject
    {
        public bool CanBePossessed { get; }
        public void PossessBy(IPlayerConsciousness playerConsciousness);
        public void RevokePossession();
    }
}
