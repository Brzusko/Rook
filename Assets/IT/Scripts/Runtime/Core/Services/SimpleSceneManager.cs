using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Managing.Scened;
using FishNet.Transporting;
using FishNet.Managing.Scened;
using IT.Interfaces;
using IT.ScriptableObjects;
using UnityEngine;

namespace IT
{
    //TODO Rewrite later to implement some external Interface
    public class SimpleSceneManager : MonoBehaviour, ISceneManager
    {
        [SerializeField] private ScenesCollection _scenesCollection;
        
        private bool _registered;
        private bool _areNetworkEventsBound;

        public Type Type => typeof(ISceneManager);
        public GameObject GameObject => gameObject;

        private void Start()
        {
            RegisterToServiceContainer();
        }

        private void OnEnable()
        {
            
            BindNetworkEvents();
        }

        private void OnDisable()
        {
            UnbindNetworkEvents();
        }

        private void OnDestroy()
        {
            UnregisterToServiceContainer();
            
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

        private void UnregisterToServiceContainer()
        {
            if(!_registered)
                return;
            
            ServiceContainer.UnregisterService<ISceneManager>();
        }

        private void BindNetworkEvents()
        {
            if(_areNetworkEventsBound)
                return;

            _areNetworkEventsBound = true;
            InstanceFinder.ServerManager.OnServerConnectionState += OnInternalServerState;
            InstanceFinder.ClientManager.OnClientConnectionState += OnInternalClientState;
        }

        private void UnbindNetworkEvents()
        {
            if(!_areNetworkEventsBound)
                return;
            
            _areNetworkEventsBound = false;
            InstanceFinder.ServerManager.OnServerConnectionState -= OnInternalServerState;
            InstanceFinder.ClientManager.OnClientConnectionState -= OnInternalClientState;
        }

        private void OnInternalServerState(ServerConnectionStateArgs args)
        {
            switch (args.ConnectionState)
            {
                case LocalConnectionState.Stopped:
                {
                    var targetSceneName = _scenesCollection.GameplayScenes.First();
                    var activeSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                    
                    if(targetSceneName != activeSceneName)
                        break;

                    StartCoroutine(UnloadMainSceneOnClient());
                    break;
                }

                case LocalConnectionState.Started:
                {
                    SceneLoadData sceneLoadData = new SceneLoadData(_scenesCollection.GameplayScenes.First());
                    InstanceFinder.SceneManager.LoadGlobalScenes(sceneLoadData);
                    break;
                }
            }
            
        }

        private void OnInternalClientState(ClientConnectionStateArgs args)
        {
            var targetSceneName = _scenesCollection.GameplayScenes.First();
            var activeSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            var isSeverCreated = ServiceContainer.Get<INetworkBridge>().IsServerCreated;
            
            if(args.ConnectionState != LocalConnectionState.Stopping || targetSceneName != activeSceneName || isSeverCreated)
                return;

            StartCoroutine(UnloadMainSceneOnClient());
        }
        
    }
}
