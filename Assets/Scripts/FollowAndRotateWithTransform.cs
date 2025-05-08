using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowAndRotateWithTransform : MonoBehaviour
{
    private Transform target;

    private void LateUpdate()
    {
        if (target != null)
        {
            transform.position = target.position;
            transform.rotation = target.rotation;
        }
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
    }
}
