using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Utils
{
    public class ObjectPlatformExluder : MonoBehaviour
    {
        private void Start()
        {
#if UNITY_STANDALONE_LINUX
            Destroy(gameObject);
#endif
        }
    }

}