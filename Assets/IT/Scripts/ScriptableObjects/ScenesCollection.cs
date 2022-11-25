using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.ScriptableObjects
{
    [CreateAssetMenu(fileName = "ScenesCollection", menuName = "IT/Scenes/ScenesCollection")]
    public class ScenesCollection : ScriptableObject
    {
        [SerializeField] private string[] _coreScenes;
        [SerializeField] private string[] _gameplayScenes;

        public IEnumerable<string> CoreScenes => _coreScenes;
        public IEnumerable<string> GameplayScenes => _gameplayScenes;
    }
}
