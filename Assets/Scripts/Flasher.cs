using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flasher : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Material material;
    private float flashAmount;
    private float t;
    private bool flashing;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        material = spriteRenderer.material;
    }

    private void Update()
    {
        if (flashing)
        {
            t += Time.deltaTime * 10f;
            flashAmount = Mathf.PingPong(t, 1f);
            material.SetFloat("_FlashAmount", flashAmount);
        }
        else
        {
            material.SetFloat("_FlashAmount", 0f);
        }
    }

    public void SetFlashing(bool flashing)
    {
        this.flashing = flashing;
    }
}
