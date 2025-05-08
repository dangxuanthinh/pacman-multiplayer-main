using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floor : MonoBehaviour
{
    [SerializeField] private float scrollSpeed;

    private Material material;
    private Vector2 textureOffset = Vector2.zero;

    private void Awake()
    {
        material = GetComponent<MeshRenderer>().material;
    }

    private void Update()
    {
        textureOffset += Vector2.one * scrollSpeed * Time.deltaTime;
        material.mainTextureOffset = textureOffset;
    }
}
