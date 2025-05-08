using System;
using System.Collections;
using UnityEngine;

public class PelletRespawner : MonoBehaviour
{
    private Material material;
    private float flashAmount;
    private Color color;
    private float t;

    private void Awake()
    {
        material = GetComponentInChildren<MeshRenderer>().material;
        color = material.GetColor("_EmissionColor");
    }

    private void Update()
    {
        t += Time.deltaTime * 5f;
        flashAmount = Mathf.Sin(t) + 1.5f;
        material.SetColor("_EmissionColor", color * flashAmount);
    }
}
