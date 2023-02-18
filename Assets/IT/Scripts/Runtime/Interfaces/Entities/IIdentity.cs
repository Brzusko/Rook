using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyNamespace
{
    public interface IIdentity<T>
    {
        public T Identity { get; }
        public void BindIdentity(T dataToBind);
    }
}
