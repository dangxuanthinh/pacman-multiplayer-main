using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField] private float speedX = 0f;
    [SerializeField] private float speedY = 0f;
    [SerializeField] private float speedZ = 0f;

    [SerializeField] private Vector3 startOffset;

    private float x = 0f;
    private float y = 0f;
    private float z = 0f;

    private void Start()
    {
        x = startOffset.x;
        y = startOffset.y;
        z = startOffset.z;
    }

    private void Update()
    {
        x += Time.deltaTime * speedX;
        y += Time.deltaTime * speedY;
        z += Time.deltaTime * speedZ;

        transform.rotation = Quaternion.Euler(x, y, z);
    }
}
