using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class CollectPelletProgressionData : AchievementProgressionData
{
    public int pelletsEaten = 0;
}


// Eat a total of 100 pellets
public class CollectPelletAchievement : Achievement
{
    private CollectPelletProgressionData collectPelletProgressionData;
    public int totalPelletsToComplete = 100;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene.name == "Gameplay")
        {
            if (GameManager.Instance != null)
            {
                Debug.Log("OnGameStart collect coin subscribed");
                GameManager.Instance.OnGameStart += OnGameStart;
            }
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (Pacman.LocalInstance != null)
        {
            Pacman.LocalInstance.OnPelletEaten -= OnPelletEaten;
        }
    }

    private void OnGameStart()
    {
        Pacman.LocalInstance.OnPelletEaten += OnPelletEaten;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnPelletEaten();
        }
    }

    private void OnPelletEaten()
    {
        if (progressionData.progressionState != AchievementState.InProgress) return;
        if (collectPelletProgressionData.pelletsEaten < totalPelletsToComplete)
        {
            collectPelletProgressionData.pelletsEaten++;
            UpdateProgressionData(collectPelletProgressionData);
        }
        if (collectPelletProgressionData.pelletsEaten >= totalPelletsToComplete)
        {
            FinishAchievement();
        }
    }

    protected override AchievementProgressionData InitializeProgressionData(string progressionDataJson)
    {
        if (string.IsNullOrEmpty(progressionDataJson))
        {
            collectPelletProgressionData = new CollectPelletProgressionData();
        }
        else
        {
            collectPelletProgressionData = JsonConvert.DeserializeObject<CollectPelletProgressionData>(progressionDataJson);
        }
        return collectPelletProgressionData;
    }

    protected override string UpdateProgressionStatusText()
    {
        if (collectPelletProgressionData.pelletsEaten == totalPelletsToComplete)
            return "Finished";
        return $"{collectPelletProgressionData.pelletsEaten}/{totalPelletsToComplete}";
    }
}
