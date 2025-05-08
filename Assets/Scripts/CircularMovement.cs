using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircularMovement : MonoBehaviour
{
    public float radius = 2f;
    public float speed = 10f;

    private Vector3 centerPosition;
    private float angle = 0f;

    void Start()
    {
        centerPosition = transform.localPosition;
    }

    void Update()
    {
        angle += speed * Time.deltaTime;

        float x = centerPosition.x + radius * Mathf.Cos(angle);
        float y = centerPosition.y + radius * Mathf.Sin(angle);

        transform.localPosition = new Vector3(x, y, 0);
    }
}
