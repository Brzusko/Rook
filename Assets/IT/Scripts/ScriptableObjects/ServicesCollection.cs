using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.ScriptableObjects
{
    [CreateAssetMenu(fileName = nameof(ServicesCollection), menuName = "IT/Services/ServicesCollection")]
    public class ServicesCollection : ScriptableObject
    {
        [SerializeField] private Service[] _servicePrefabs;

        public IEnumerable ServicePrefabs => _servicePrefabs;
    }
}
