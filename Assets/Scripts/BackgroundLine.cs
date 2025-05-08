using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundLine : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    private MaterialPropertyBlock materialProperty;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        materialProperty = new MaterialPropertyBlock();
    }

    private void Update()
    {
        meshRenderer.GetPropertyBlock(materialProperty);
        materialProperty.SetColor("_Color", DiscoLight.Instance.CurrentColor * 2f);
        materialProperty.SetColor("_EmissionColor", DiscoLight.Instance.CurrentColor * 2f);
        meshRenderer.SetPropertyBlock(materialProperty);
    }
}
