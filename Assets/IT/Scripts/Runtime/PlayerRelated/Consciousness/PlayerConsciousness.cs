using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using IT.Interfaces;
using UnityEngine;

public class PlayerConsciousness : NetworkBehaviour, IPlayerConsciousness
{
    [SerializeField]
    private NetworkObject _networkObject;
    public NetworkObject NetworkObject => _networkObject;
}
