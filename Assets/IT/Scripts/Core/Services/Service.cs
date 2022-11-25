using System;
using System.Collections;
using System.Collections.Generic;
using IT.Interfaces;
using UnityEngine;


namespace IT
{
    public abstract class Service : MonoBehaviour, IService
    {
        public virtual Type Type { get; }
        public GameObject GameObject => gameObject;
    }
}