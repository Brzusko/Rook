using System.Collections;
using System.Collections.Generic;
using IT.Interfaces;
using UnityEngine;

namespace IT.Interfaces
{
    public interface INetworkSettingsService : IService
    {
        public ushort Port { get; set; }
        public string Address { get; set; }
        public int MaxClients { get; set; }
        public bool AsServer { get; set; }
    }
}
