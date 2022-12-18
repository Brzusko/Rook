using System;
using System.Collections;
using System.Collections.Generic;
using IT.Interfaces;
using UnityEngine;

namespace IT.Utils
{
    public class SimpleServerRunner : MonoBehaviour
    {
        private void Start()
        {
#if UNITY_STANDALONE_LINUX
            ServiceContainer.Get<INetworkBridge>().StartServer();
#endif
        }
    }
}
