using System.Collections;
using System.Collections.Generic;
using Mono.CSharp.Linq;
using UnityEngine;

namespace IT.Interfaces
{
    public interface IFileProcessor<out T> where T : class, new()
    {
        public void WriteContent<TW>(TW newContent) where TW : class;
        public void SaveCache();
        public void LoadFile();

        public void Prune();
        public T GrabFileContent();
    }
}
