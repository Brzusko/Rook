using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using IT.Interfaces;
using UnityEngine;

public class PlayerEntity : NetworkBehaviour, IEntityToPossess
{
    public event Action<bool> ServerPossessChanged;
    public event Action<bool> ClientPossessChanged;
    public bool CanBePossessed => OwnerId == -1;
    
    [Server]
    public bool PossessBy(IPlayerConsciousness playerConsciousness)
    {
        if(!CanBePossessed || playerConsciousness.HasPossession)
            return false;

        GiveOwnership(playerConsciousness.NetworkObject.Owner);
        ServerPossessChanged?.Invoke(true);
        return true;
    }

    [Server]
    public void RevokePossession()
    {
        if(CanBePossessed)
            return;
        
        RemoveOwnership();
        ServerPossessChanged?.Invoke(false);
    }
    
    public override void OnOwnershipClient(NetworkConnection prevOwner)
    {
        base.OnOwnershipClient(prevOwner);
        ClientPossessChanged?.Invoke(IsOwner);
    }
}
