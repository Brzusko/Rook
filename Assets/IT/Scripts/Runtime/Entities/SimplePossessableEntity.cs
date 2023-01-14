using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using FishNet.Connection;
using FishNet.Object;
using IT.Interfaces;
using UnityEngine;

public class SimplePossessableEntity : NetworkBehaviour, IEntityToPossess
{
    [SerializeField] private CameraSpawner _cameraSpawner;
    
    private CinemachineVirtualCamera _virtualCamera;
    private bool _isPossessed;
    public bool CanBePossessed => !_isPossessed;
    
    private void OnDestroy()
    {
        if(_virtualCamera != null)
            Destroy(_virtualCamera.gameObject);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        
        _virtualCamera = _cameraSpawner.SpawnCamera();
        Transform cameraTarget = transform;
        _virtualCamera.LookAt = cameraTarget;
        _virtualCamera.Follow = cameraTarget;
    }

    [Server]
    public void PossessBy(IPlayerConsciousness playerConsciousness)
    {
        if(_isPossessed)
            return;
        
        NetworkObject.GiveOwnership(playerConsciousness.NetworkObject.Owner);

        _isPossessed = true;
    }
    
    [Server]
    public void RevokePossession()
    {
        if(!_isPossessed)
            return;

        NetworkObject.RemoveOwnership();
        
        _isPossessed = false;
    }

    public override void OnOwnershipClient(NetworkConnection prevOwner)
    {
        base.OnOwnershipClient(prevOwner);

        if (IsOwner)
        {
            _virtualCamera.Priority = 10;
            return;
        }

        _virtualCamera.Priority = 0;
    }
}
