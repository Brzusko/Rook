using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using IT.Interfaces;
using UnityEngine;

public class PlayerConsciousness : NetworkBehaviour, IPlayerConsciousness
{
    private IPlayersConsciousness _playersConsciousness;
    private IEntityToPossess _currentPossession;
    
    [Server]
    public void Initialize(IPlayersConsciousness playersConsciousness)
    {
        _playersConsciousness = playersConsciousness;
    }
    
    [Server]
    public void Possess(IEntityToPossess entityToPossess)
    {
        if(!entityToPossess.CanBePossessed || _currentPossession != null)
            return;

        _currentPossession = entityToPossess;
        entityToPossess.PossessBy(this);
    }
    
    [Server]
    public void RevokeCurrentPossession(bool clearCache = false)
    {
        if(_currentPossession == null)
            return;
        
        _currentPossession.RevokePossession();

        if (clearCache)
            _currentPossession = null;
    }
}
