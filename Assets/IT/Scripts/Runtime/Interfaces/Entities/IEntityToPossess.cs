using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Interfaces
{
    public interface IEntityToPossess : INetworkObject
    {
        public event Action<bool> ServerPossessChanged;
        public event Action<bool> ClientPossessChanged; 
        public bool CanBePossessed { get; }
        public bool PossessBy(IPlayerConsciousness playerConsciousness);
        public void RevokePossession();
    }
}
