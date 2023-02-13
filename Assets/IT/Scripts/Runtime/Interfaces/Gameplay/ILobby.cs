using System;
using System.Collections.Generic;
using FishNet.Connection;

namespace IT.Interfaces
{
    public interface ILobby<T>
    {
        public event Action EveryoneReady;
        
        public void OpenLobby();
        public void CloseLobby();
        public IEnumerable<T> FetchWaiters();
        public void RequestData();
    }
}
