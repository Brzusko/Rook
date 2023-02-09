using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IT.ScriptableObjects
{
    [CreateAssetMenu(fileName = "ScenesCollection", menuName = "IT/Scenes/ScenesCollection")]
    public class ScenesCollection : ScriptableObject
    {
        [SerializeField] private string[] _coreScenes;
        [SerializeField] private string _offlineSceneID;
        [SerializeField] private string _onlineSceneID;

        public IEnumerable<string> CoreScenes => _coreScenes;
        public string OfflineScene => _offlineSceneID;
        public string OnlineScene => _onlineSceneID;
    }
}
