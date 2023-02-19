using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using IT.Data;
using IT.Interfaces;
using IT.ScriptableObjects.UI;
using UnityEngine;

public class PlayerIdentityComponent : NetworkBehaviour, IIdentity<PlayerIdentity>
{
    private static readonly int _baseColor = Shader.PropertyToID("_BaseColor");
    
    [SerializeField] private Renderer _flagRenderer;
    [SerializeField] private GameplayBinding _gameplayBinding;
    [SerializeField] private GameObject _entityGameObject;
    
    [SyncVar(OnChange = nameof(OnColorChange))] private Color _color;
    private IEntityToPossess _entity;
    
    public PlayerIdentity Identity => new() { Color = _color };

    private void Awake()
    {
        _entity = _entityGameObject.GetComponent<IEntityToPossess>();
        _entity.ClientPossessChanged += OnClientPosses;
    }

    private void OnDestroy()
    {
        _entity.ClientPossessChanged -= OnClientPosses;
    }

    private void OnClientPosses(bool posses)
    {
        if(!posses)
            return;
        
        Debug.Log("Event?");
        _gameplayBinding.FireSetMain(NetworkObject.ObjectId);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        _gameplayBinding.FireCreatePlayerIndicator(NetworkObject.ObjectId, _color);
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        
        _gameplayBinding.FireRemovePlayerIndicator(NetworkObject.ObjectId);
    }

    private void OnColorChange(Color prev, Color next, bool asServer)
    {
        _flagRenderer.material.SetColor(_baseColor, next);
    }

    public void BindIdentity(PlayerIdentity dataToBind)
    {
        _color = dataToBind.Color;
    }
}
