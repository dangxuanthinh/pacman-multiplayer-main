using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PortalParticle : MonoBehaviour
{
    private List<ParticleSystem> childParticles = new List<ParticleSystem>();

    public void SetColor(Color color)
    {
        if (childParticles.Count == 0)
        {
            childParticles = GetComponentsInChildren<ParticleSystem>().ToList();
        }
        foreach (ParticleSystem particle in childParticles)
        {
            var main = particle.main;
            main.startColor = color;
        }
    }
}
