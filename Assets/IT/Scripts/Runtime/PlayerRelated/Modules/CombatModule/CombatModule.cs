using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class CombatModule : NetworkBehaviour
{
    [SerializeField]
    private float _attackSpeedInSeconds;

    public float AttackSpeed
    {
        get => _attackSpeedInSeconds;
        set => _attackSpeedInSeconds = value;
    }
    
    
}
