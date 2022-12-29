using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Object;
using UnityEngine;

public class TestSynchronizer : NetworkBehaviour
{
    [SerializeField] private Transform _targetTransform;
    public override void OnStartServer()
    {
        base.OnStartServer();

        InstanceFinder.TimeManager.OnPostTick += OnPostTick;
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        InstanceFinder.TimeManager.OnPostTick -= OnPostTick;
    }

    private void OnPostTick()
    {
        RecivePosition(_targetTransform.position);
    }
    
    [ObserversRpc(IncludeOwner = false)]
    private void RecivePosition(Vector3 position)
    {
        _targetTransform.position = position;
    }
}
