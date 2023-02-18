using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using IT.Interfaces;
using UnityEngine;

public class PlayerConsciousness : NetworkBehaviour, IPlayerConsciousness
{
    private IEntityToPossess _boundPossession;
    public bool HasPossession => _boundPossession is { CanBePossessed: false };

    public override void OnStopServer()
    {
        base.OnStopServer();
        
        if(_boundPossession == null)
            return;
        
        ServerManager.Despawn(_boundPossession.NetworkObject);
    }

    public void BindEntity(IEntityToPossess entityToPossess)
    {
        _boundPossession = entityToPossess;
    }

    public void Possess()
    {
        if(_boundPossession is not { CanBePossessed: true })
            return;
        
        _boundPossession.PossessBy(this);
    }

    public void Unpossess()
    {
        if(_boundPossession is not { CanBePossessed: false })
            return;
        
        _boundPossession.RevokePossession();
    }

    public void Clear()
    {
        if(!IsServer)
            return;
        
        ServerManager.Despawn(NetworkObject);
        
        if(_boundPossession != null)
            ServerManager.Despawn(_boundPossession.NetworkObject);
    }
}
