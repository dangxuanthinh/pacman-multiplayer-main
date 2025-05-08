using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;

    public static AudioManager Instance;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.pitch = s.pitch;
            s.source.name = s.name;
            s.source.volume = s.volume;
            s.source.loop = s.loop;
            s.source.outputAudioMixerGroup = s.outputGroup;
        }
    }

    public void StopAllSounds()
    {
        foreach (Sound s in sounds)
        {
            s.source.Stop();
        }
    }

    public void HandleAudioOnGamePaused()
    {
        foreach (Sound s in sounds)
        {
            if (s.stopOnGamePaused) s.source.Pause();
        }
    }

    public void HandleAudioOnGameUnPaused()
    {
        foreach (Sound s in sounds)
        {
            if (s.stopOnGamePaused) s.source.UnPause();
        }
    }

    public void Play(string name, bool resetIfAlreadyPlaying = false)
    {
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogWarning("Sound name is null or empty");
            return;
        }
        Sound s = GetSound(name);
        if (s.source.isPlaying)
        {
            if (resetIfAlreadyPlaying)
            {
                s.source.Play();
                return;
            }
            else
            {
                return;
            }
        }
        else
        {
            s.source.Play();
        }
    }

    public void Stop(string name)
    {
        Sound s = GetSound(name);
        s.source.Stop();
    }

    public void Pause(string name)
    {
        Sound s = GetSound(name);
        s.source.Pause();
    }

    public void UnPause(string name)
    {
        Sound s = GetSound(name);
        s.source.UnPause();
    }

    public Sound GetSound(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogError("Can't find sound with name: " + name);
        }
        return s;
    }

    public void CleanupAllSounds()
    {
        foreach (Sound s in sounds)
        {
            if (s.persistOnSceneLoad) continue;
            s.source.Stop();
        }
    }
}


