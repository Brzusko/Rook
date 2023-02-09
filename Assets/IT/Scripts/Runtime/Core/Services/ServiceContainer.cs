using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IT.Interfaces;
using IT.ScriptableObjects;
using UnityEngine;

namespace IT
{
    public class ServiceContainer : MonoBehaviour
    {
        private static readonly  Dictionary<Type, IService> _services = new Dictionary<Type, IService>();
  
        private void OnDestroy()
        {
            UnloadServices();
        }

        public static void RegisterService<T>(T service) where T : class, IService
        {
            if(_services.ContainsKey(typeof(T)))
                return;
            
            _services.Add(typeof(T), service);
        }

        public static void UnregisterService<T>() where T : class, IService
        {
            if(!_services.ContainsKey(typeof(T)))
                return;

            _services.Remove(typeof(T));
        }
        
        public static void FetchDependency()
        {
            if(_services.Count == 0)
                return;

            foreach (var service in _services.Where(service => service.Value is IServiceWithDependency))
            {
                ((IServiceWithDependency)service.Value).FetchDependency();
            }
        }
        
        public static T Get<T>() where T : IService
        {
            return (T)_services[typeof(T)];
        }
        
        public static bool Contains<T>()
        {
            return _services.ContainsKey((typeof(T)));
        }
        
        private void UnloadServices()
        {
            foreach (var service in _services)
                if(!service.Value.GameObject)
                    Destroy(service.Value.GameObject);

            _services.Clear();
        }
    }
}
