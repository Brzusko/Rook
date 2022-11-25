using System;
using UnityEngine;

namespace IT.Interfaces
{
    public interface IService
    {
        public Type Type { get; }
        public GameObject GameObject { get; }
    }
}
