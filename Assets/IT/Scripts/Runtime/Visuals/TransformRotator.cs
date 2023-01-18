using System.Collections;
using System.Collections.Generic;
using FishNet.Utility.Extension;
using UnityEngine;

public class TransformRotator : MonoBehaviour
{
    [SerializeField] private bool _rotateXAxis;
    [SerializeField] private bool _rotateYAxis;
    [SerializeField] private bool _rotateZAxis;
    [SerializeField] private Transform _transformToUpdate;

    public void Rotate(Vector3 worldPoint, float rotateSpeed, float deltaTime)
    {
        Quaternion desiredRotation = Quaternion.LookRotation(worldPoint - transform.position);
        Vector3 eulerAngles = desiredRotation.eulerAngles;
        eulerAngles.x = _rotateXAxis ? eulerAngles.x : 0f;
        eulerAngles.y = _rotateYAxis ? eulerAngles.y : 0f;
        eulerAngles.z = _rotateZAxis ? eulerAngles.z : 0f;
        desiredRotation = Quaternion.Euler(eulerAngles);

        _transformToUpdate.rotation = Quaternion.Slerp(_transformToUpdate.rotation, desiredRotation, rotateSpeed * deltaTime);
    }

    public void RotateY(float yValue, float rotateSpeed, float deltaTime)
    {
        Quaternion desiredRotation = Quaternion.Euler(0f, yValue, 0f);
        _transformToUpdate.rotation =
            Quaternion.Slerp(_transformToUpdate.rotation, desiredRotation, rotateSpeed * deltaTime);
    }
}
