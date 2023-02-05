using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using FishNet;
using FishNet.Managing.Timing;
using FishNet.Object;
using IT.Input;
using IT.Interfaces;
using UnityEngine;

public class CameraRaycaster : NetworkBehaviour, IRaycaster
{
    [SerializeField] private GameObject _entityGameObject;
    [SerializeField] private PlayerInputReader _playerInputReader;
    
    private IEntityToPossess _entity;
    private bool _areEntityEventsBound;
    private bool _areTickEventsBound;
    private bool _foundRaycastHitThisTick;
    private Camera _camera;

    private RaycastHit _recentRaycastHit;

    public bool FoundRaycastHitThisTick => _foundRaycastHitThisTick;
    public RaycastHit RaycastHit => _recentRaycastHit;
    
    public override void OnStartClient()
    {
        base.OnStartClient();
        InitializeOnce();
        BindEntityEvents();
    }

    private void OnDestroy()
    {
        UnbindEntityEvents();
        UnbindTickEvents();
    }

    private void InitializeOnce()
    {
        _entity = _entityGameObject.GetComponent<IEntityToPossess>();
    }

    private void BindEntityEvents()
    {
        if(_areEntityEventsBound)
            return;

        _entity.ClientPossessChanged += OnClientPossessChanged;
        _areEntityEventsBound = true;
    }

    private void UnbindEntityEvents()
    {
        if(!_areEntityEventsBound)
            return;

        _entity.ClientPossessChanged -= OnClientPossessChanged;
        _areEntityEventsBound = false;
    }

    private void BindTickEvents()
    {
        if(_areTickEventsBound)
            return;
        
        CinemachineCore.CameraUpdatedEvent.AddListener(OnCameraTick);
        _areTickEventsBound = true;
    }

    private void UnbindTickEvents()
    {
        if(!_areTickEventsBound)
            return;
        
        CinemachineCore.CameraUpdatedEvent.RemoveListener(OnCameraTick);
        _areTickEventsBound = false;
    }

    private void OnCameraTick(CinemachineBrain brain)
    {
        Camera currentCamera = brain.OutputCamera;
        Ray screenPointToRay = currentCamera.ScreenPointToRay(_playerInputReader.PointerPosition);
        _foundRaycastHitThisTick = Physics.Raycast(screenPointToRay, out RaycastHit hit);
        
        if(!_foundRaycastHitThisTick)
            return;
        
        _recentRaycastHit = hit;
    }

    private void OnClientPossessChanged(bool gainedPossession)
    {
        if (gainedPossession)
        {
            BindTickEvents();
            return;
        }
        
        UnbindEntityEvents();
    }
}
