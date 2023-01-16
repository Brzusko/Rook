using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using FishNet.Object;
using IT.Interfaces;
using UnityEngine;

public class TopDownCameraController : NetworkBehaviour, ICameraController
{
    [SerializeField] private GameObject _cameraPrefabToSpawn;
    [SerializeField] private Transform _lookAt;
    [SerializeField] private Transform _follow;
    [SerializeField] private GameObject _entityGameObject;
    
    private CinemachineVirtualCamera _virtualCamera;
    private IEntityToPossess _entity;
    private bool _areEventsBound;

    public Vector3 CameraForward => _virtualCamera.State.CorrectedOrientation * Vector3.forward;

    public override void OnStartClient()
    {
        base.OnStartClient();
        
        InitializeOnce();
        BindEvents();
    }

    private void OnDestroy()
    {
        UnbindEvents();
    }

    private void InitializeOnce()
    {
        _entity = _entityGameObject.GetComponent<IEntityToPossess>();
        SpawnCamera();
    }

    private void BindEvents()
    {
        if(_areEventsBound)
            return;

        _entity.ClientPossessChanged += OnClientPossessChanged;
        _areEventsBound = true;
    }

    private void UnbindEvents()
    {
        if(!_areEventsBound)
            return;

        _entity.ClientPossessChanged -= OnClientPossessChanged;
        _areEventsBound = false;
    }

    private void SpawnCamera()
    {
        GameObject cameraInstance = Instantiate(_cameraPrefabToSpawn);
        CinemachineVirtualCamera virtualCamera = cameraInstance.GetComponent<CinemachineVirtualCamera>();

        if (virtualCamera == null)
        {
            Debug.LogError("Please provide correct camera prefab");
            Destroy(cameraInstance);
            return;
        }

        _virtualCamera = virtualCamera;
        _virtualCamera.Priority = 0;
        _virtualCamera.LookAt = _lookAt;
        _virtualCamera.Follow = _follow;
    }

    private void EnableCamera()
    {
        _virtualCamera.Priority = 999;
        enabled = true;
    }

    private void DisableCamera()
    {
        _virtualCamera.Priority = 0;
        enabled = false;
    }

    private void OnClientPossessChanged(bool gainPossession)
    {
        if (gainPossession)
        {
            EnableCamera();
            return;
        }
        
        DisableCamera();
    }
}
