using DG.Tweening;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;
    private Image blackCover;
    private const float FADE_DURATION = 0.3f;
    private UnityAction OnSceneLoadedAction;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(gameObject);
        blackCover = GetComponentInChildren<Image>();
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        blackCover.raycastTarget = false;
        blackCover.DOFade(0f, FADE_DURATION);
        OnSceneLoadedAction?.Invoke();
        AudioManager.Instance.CleanupAllSounds();
    }

    public void LoadScene(string sceneName, UnityAction OnSceneLoaded = null)
    {
        blackCover.raycastTarget = true;
        blackCover.DOFade(1f, FADE_DURATION).SetUpdate(true).OnComplete(() =>
        {
            SceneManager.LoadScene(sceneName);
        });
        OnSceneLoadedAction = OnSceneLoaded;
        string savedData = DataPersistenceManager.Instance.SaveGame();
        UserManager.Instance.SaveGame(savedData);
    }

    public void LoadSceneNetwork(string sceneName)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}
