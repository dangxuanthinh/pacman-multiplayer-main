using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Sound
{
    public AudioClip clip;
    public bool loop;
    public bool stopOnGamePaused;
    public bool persistOnSceneLoad;
    public string name;
    [Range(0f, 1f)]
    public float volume = 1;
    [Range(0.1f, 3f)]
    public float pitch = 1;
    [HideInInspector]
    public AudioSource source;
    public AudioMixerGroup outputGroup;
}
