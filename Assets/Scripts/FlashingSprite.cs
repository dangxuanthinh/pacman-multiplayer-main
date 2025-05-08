using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashingSprite : MonoBehaviour
{
    [SerializeField] private float timeBtwFlashes = 0.35f;
    private float elapsedTime = 0f;
    private MeshRenderer meshRenderer;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= timeBtwFlashes)
        {
            elapsedTime = 0f;
            meshRenderer.enabled = !meshRenderer.enabled;
        }
    }
}
