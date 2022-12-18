using System;
using System.Collections;
using System.Collections.Generic;
using IT.Interfaces;
using Mono.CSharp;
using UnityEngine;


namespace IT
{
    public abstract class Service : MonoBehaviour, IService
    {
        public abstract Type Type { get; }
        public GameObject GameObject => gameObject;
    }
}