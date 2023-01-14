using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _cameraPrefab;

    public CinemachineVirtualCamera SpawnCamera()
    {
        return Instantiate(_cameraPrefab).GetComponent<CinemachineVirtualCamera>();
    }
}
