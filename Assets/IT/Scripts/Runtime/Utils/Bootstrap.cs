using System;
using System.Linq;
using System.Threading.Tasks;
using IT.Data;
using IT.Interfaces;
using IT.ScriptableObjects;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace IT.Utils
{
    public class Bootstrap : MonoBehaviour
    {
        [SerializeField] private ScenesCollection _scenesToLoad;
        private Scene _bootstrapSceneRef;
        private void Awake()
        {
            _bootstrapSceneRef = SceneManager.GetActiveScene();
        }

        private async void Start()
        {
            await LoadScenes();
            ServiceContainer.FetchDependency();

#if UNITY_STANDALONE_LINUX
            ServiceContainer.Get<INetworkBridge>().StartServer();
#endif
            await UnloadSceneBySceneRef(_bootstrapSceneRef);
        }

        private async Task LoadScenes()
        {
            foreach (var sceneName in _scenesToLoad.CoreScenes)
            {
                try
                {
                    await LoadSceneByName(sceneName);
                }
                catch (Exception ex)
                {
                    QuitOnError(ex);
                }
            }
        }

        private async Task LoadSceneByName(string sceneName)
        {
            var asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            await WaitForAsyncOperation(asyncOperation);
        }

        private async Task UnloadSceneBySceneRef(Scene sceneRef)
        {
            var asyncOperation = SceneManager.UnloadSceneAsync(sceneRef);
            await WaitForAsyncOperation(asyncOperation);
        }

        private async Task WaitForAsyncOperation(AsyncOperation asyncOperation)
        {
            while (!asyncOperation.isDone)
                await Task.Yield();
        }
        
        private void QuitOnError(Exception ex = null)
        {
            if(ex != null)
                Debug.LogException(ex);
            
            Application.Quit();
        }
    }
}
