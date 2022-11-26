using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Data
{
    public record NetworkSettingsWriteData(ushort Port, string Address, int MaxClients, bool AsDedicatedServer);
}
