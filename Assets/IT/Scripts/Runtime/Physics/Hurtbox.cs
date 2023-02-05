using System.Collections;
using System.Collections.Generic;
using Mono.CSharp;
using UnityEngine;

public class Hurtbox : MonoBehaviour
{
    [SerializeField] private Collider _collider;
    public bool DetectHitbox<T>(out T component) where T : class
    {
        component = null;
        Transform colliderTransform = _collider.transform;
        Quaternion colliderRotation = colliderTransform.rotation;
        Bounds colliderBounds = _collider.bounds;
        int mask = 1 << LayerMask.NameToLayer("HitBox");

        Collider[] colliders =
            Physics.OverlapBox(colliderBounds.center, colliderBounds.extents * 0.5f, colliderRotation, mask);

        if (colliders.Length == 0)
            return false;

        foreach (Collider collider in colliders)
        {
            if (collider.TryGetComponent(out T candidate))
            {
                component = candidate;
                return true;
            }
        }

        return false;
    }
    
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        
        Gizmos.DrawWireCube(_collider.bounds.center, _collider.bounds.extents);
    }
}
