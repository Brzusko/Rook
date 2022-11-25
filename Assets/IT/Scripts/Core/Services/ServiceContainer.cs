using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IT.Interfaces;
using UnityEngine;

namespace IT
{
    public class ServiceContainer : MonoBehaviour
    {
        private static readonly  Dictionary<Type, IService> _services = new Dictionary<Type, IService>();
        [SerializeField] private Service[] _servicePrefabs;
        private void Awake()
        {
            LoadServices();
            FetchDependency();
        }

        private void OnDestroy()
        {
            UnloadServices();
        }

        private void LoadServices()
        {
            foreach (var servicePrefab in _servicePrefabs)
            {
                if(servicePrefab is not IService)
                    continue;

                var serviceInstance = Instantiate(servicePrefab);
                var serviceInterface = (IService)serviceInstance;
                
                if(_services.ContainsKey(serviceInterface.Type))
                    Destroy(serviceInstance.gameObject);

                _services.Add(serviceInstance.Type, serviceInstance);
            }
        }

        private void FetchDependency()
        {
            if(_services.Count == 0)
                return;

            foreach (var service in _services.Where(service => service.Value is IServiceWithDependency))
            {
                ((IServiceWithDependency)service.Value).FetchDependency();
            }
        }

        private void UnloadServices()
        {
            foreach (var service in _services)
                Destroy(service.Value.GameObject);

            _services.Clear();
        }

        private static T Get<T>() where T : IService
        {
            return (T)_services[typeof(T)];
        }

        private static bool Contains<T>()
        {
            return _services.ContainsKey((typeof(T)));
        }
    }
}
