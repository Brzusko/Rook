using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Interfaces
{
    public interface IVisuals<T>
    {
        public T Visuals { get; }
        public void SetVisuals(T visuals);
    }
}
