using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using IT.Data;
using MyNamespace;
using UnityEngine;

public class PlayerIdentityComponent : NetworkBehaviour, IIdentity<PlayerIdentity>
{
    private static readonly int _baseColor = Shader.PropertyToID("_BaseColor");
    
    [SerializeField] private Renderer _flagRenderer;
    
    [SyncVar(OnChange = nameof(OnColorChange))] private Color _color;

    public PlayerIdentity Identity => new() { Color = _color };
    
    private void OnColorChange(Color prev, Color next, bool asServer)
    {
        _flagRenderer.material.SetColor(_baseColor, next);
    }

    public void BindIdentity(PlayerIdentity dataToBind)
    {
        _color = dataToBind.Color;
    }
}
