using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Game Settings")]
    public class GameSettings : ScriptableObject
    {
        [SerializeField] private int _readyPlayersToStart;
        [SerializeField] private int _pointsGainPerTick;
        [SerializeField] private int _pointsToWin;
        [SerializeField] private int _minimumPlayersToPlay;

        public int ReadyPlayersToStart => _readyPlayersToStart;
        public int PointsGainPerTick => _pointsGainPerTick;
        public int PointsToWin => _pointsToWin;
        public int MinimunPlayersToPlay => _minimumPlayersToPlay;
    }

}