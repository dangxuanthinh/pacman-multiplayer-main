using Mono.CSharp;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Pellet : NetworkBehaviour
{
    private Material material;
    private float intensity;

    private void Awake()
    {
        material = GetComponent<MeshRenderer>().material;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlayRespawnEffect();
        }
    }

    public void PlayRespawnEffect()
    {
        StartCoroutine(PlayRespawnEffectCoroutine());
    }

    IEnumerator PlayRespawnEffectCoroutine()
    {
        Color color = material.color;
        float orignalIntensity = 2f;
        intensity = 3f;
        float duration = 1f;
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            intensity = Mathf.Lerp(3f, orignalIntensity, elapsedTime / duration);
            material.SetColor("_EmissionColor", color * intensity);
            yield return null;
        }
    }
}
