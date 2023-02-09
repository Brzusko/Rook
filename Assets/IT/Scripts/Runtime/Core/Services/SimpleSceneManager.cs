using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FishNet;
using FishNet.Managing.Client;
using FishNet.Managing.Scened;
using FishNet.Transporting;
using FishNet.Managing.Scened;
using FishNet.Managing.Server;
using IT.Interfaces;
using IT.ScriptableObjects;
using UnityEngine;
using UnityEngine.SceneManagement;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

namespace IT
{
    //TODO Rewrite later to implement some external Interface
    public class SimpleSceneManager : MonoBehaviour, ISceneManager
    {
        [SerializeField] private ScenesCollection _scenesCollection;
        
        private bool _registered;
        private bool _areNetworkEventsBound;
        private ServerManager _serverManager;
        private ClientManager _clientManager;

        public Type Type => typeof(ISceneManager);
        public GameObject GameObject => gameObject;

        private void Start()
        {
            _serverManager = InstanceFinder.ServerManager;
            _clientManager = InstanceFinder.ClientManager;
            
            RegisterToServiceContainer();
            BindNetworkEvents();
        }

        private void OnDestroy()
        {
            UnbindNetworkEvents();
        }

        private IEnumerator UnloadMainSceneOnClient()
        {
            UnityEngine.SceneManagement.Scene activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            AsyncOperation asyncOP = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(activeScene);

            while (asyncOP.isDone != false)
                yield return new WaitForEndOfFrame();
        }
        
        private void RegisterToServiceContainer()
        {
            if(_registered)
                return;
            
            ServiceContainer.RegisterService<ISceneManager>(this);
            _registered = true;
        }
        
        private void BindNetworkEvents()
        {
            if(_areNetworkEventsBound)
                return;

            _areNetworkEventsBound = true;
            _serverManager.OnServerConnectionState += OnServerConnectionState;
            _clientManager.OnClientConnectionState += OnClientConnectionState;
        }

        private void UnbindNetworkEvents()
        {
            if(!_areNetworkEventsBound)
                return;
            
            _areNetworkEventsBound = false;
            _serverManager.OnServerConnectionState -= OnServerConnectionState;
            _clientManager.OnClientConnectionState -= OnClientConnectionState;
        }

        private async void OnServerConnectionState(ServerConnectionStateArgs args)
        {
            if (args.ConnectionState == LocalConnectionState.Stopped)
            {
                await LoadOfflineScene();
                return;
            }

            if (args.ConnectionState == LocalConnectionState.Started)
            {
                SceneLoadData sceneLoadData = new SceneLoadData(_scenesCollection.OnlineScene);
                InstanceFinder.SceneManager.LoadGlobalScenes(sceneLoadData);
            }
        }

        private async void OnClientConnectionState(ClientConnectionStateArgs args)
        {
            if (args.ConnectionState == LocalConnectionState.Starting)
            {
                await UnloadOfflineScene();
                return;
            }
            
            if (args.ConnectionState == LocalConnectionState.Stopped)
            {
                INetworkBridge networkBridge = ServiceContainer.Get<INetworkBridge>();
            
                if(networkBridge.IsServerCreated)
                    return;

                await LoadOfflineScene();
            }
        }

        private async Task LoadOfflineScene()
        {
            await LoadMainScene(_scenesCollection.OfflineScene, _scenesCollection.OnlineScene);
        }

        private async Task UnloadOfflineScene()
        {
            Scene offlineScene = SceneManager.GetSceneByName(_scenesCollection.OfflineScene);
            
            if(offlineScene.buildIndex == -1 || !offlineScene.isLoaded)
                return;

            AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(offlineScene);

            while (!unloadOperation.isDone)
                await Task.Yield();
        }
        
        private async Task LoadMainScene(string sceneToLoad, string sceneNameToCompare)
        {
            Scene activeScene = SceneManager.GetActiveScene();
            
            if(activeScene.name == sceneToLoad)
                return;
            
            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(_scenesCollection.OfflineScene, LoadSceneMode.Additive);
            
            while (!loadOperation.isDone)
                await Task.Yield();

            Scene loadedScene = SceneManager.GetSceneByName(sceneToLoad);
            SceneManager.SetActiveScene(loadedScene);
            
            if(activeScene.name != sceneNameToCompare)
                return;

            AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(activeScene);
            
            while (!unloadOperation.isDone)
                await Task.Yield();
        }
    }
}
