using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Game Settings")]
    public class GameSettings : ScriptableObject
    {
        [SerializeField] private int _readyPlayersToStart;
        [SerializeField] private float _pointsGainPerTick;
        [SerializeField] private float _pointsToWin;

        public int ReadyPlayersToStart => _readyPlayersToStart;
        public float PointsGainPerTick => _pointsGainPerTick;
        public float PointsToWin => _pointsToWin;
    }

}