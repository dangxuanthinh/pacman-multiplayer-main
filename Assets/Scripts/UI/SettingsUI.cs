using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider effectsSlider;
    [SerializeField] private Button confirmButton;
    public GameObject panel;

    private void Awake()
    {
        confirmButton.onClick.AddListener(() =>
        {
            PlayerPrefs.SetFloat("MusicVolume", musicSlider.value);
            PlayerPrefs.SetFloat("EffectVolume", effectsSlider.value);
            panel.SetActive(false);
        });

        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        effectsSlider.onValueChanged.AddListener(SetEffectsVolume);
    }

    private void Start()
    {
        audioMixer.SetFloat("MusicVolume", PlayerPrefs.GetFloat("MusicVolume", 0));
        audioMixer.SetFloat("EffectVolume", PlayerPrefs.GetFloat("EffectVolume", 0));

        musicSlider.value = GetAudioMixerVolume("MusicVolume");
        effectsSlider.value = GetAudioMixerVolume("EffectVolume");
    }

    public void SetMusicVolume(float volume)
    {
        audioMixer.SetFloat("MusicVolume", volume);
    }

    public void SetEffectsVolume(float volume)
    {
        audioMixer.SetFloat("EffectVolume", volume);
    }

    private float GetAudioMixerVolume(string name)
    {
        float volume;
        bool exist = audioMixer.GetFloat(name, out volume);
        if (exist)
            return volume;
        else
            return 0;
    }
}
