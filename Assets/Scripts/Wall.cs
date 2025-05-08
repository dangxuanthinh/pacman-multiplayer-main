using QFSW.QC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
    public float scaleSpeed = 1.5f;
    public float scaleAmount = 1f;
    private Vector3 originalScale;
    public bool isScaleRandom = true;
    private Vector3 targetScale;
    private Transform modelTransform;

    bool alwaysScaleRandom;

    private void Awake()
    {
        modelTransform = transform.GetChild(0);
    }

    void Start()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            alwaysScaleRandom = !alwaysScaleRandom;
        }


        if (isScaleRandom)
        {
            float perlin = Mathf.PerlinNoise(transform.position.x / 2f + Time.time, transform.position.y / 2f + Time.time);
            targetScale = new Vector3(originalScale.x, originalScale.y, originalScale.z + perlin * scaleAmount);
        }
        else
        {
            targetScale = originalScale;
        }
        modelTransform.localScale = Vector3.Lerp(modelTransform.localScale, targetScale, Time.deltaTime * scaleSpeed);
    }

    private void FixedUpdate()
    {
        if (alwaysScaleRandom)
        {
            isScaleRandom = true;
            return;
        }
        isScaleRandom = GameManager.Instance.IsAnyGhostFrightened();
    }
}
