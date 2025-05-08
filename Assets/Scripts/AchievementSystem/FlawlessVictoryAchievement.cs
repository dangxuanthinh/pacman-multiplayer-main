using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Win without dying a single time
public class FlawlessVictoryAchievement : Achievement
{
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
                GameManager.Instance.OnGameVictory += OnVictory;
            }
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameVictory -= OnVictory;
        }
    }

    private void OnVictory()
    {
        if (Pacman.LocalInstance != null)
        {
            int remainingLives = GameManager.Instance.playerRemainingLivesTable[Pacman.LocalInstance.OwnerClientId];
            if (remainingLives == GameManager.Instance.TotalLives)
            {
                UpdateProgressionData(progressionData);
                FinishAchievement();
            }
        }
    }

    protected override AchievementProgressionData InitializeProgressionData(string progressionDataJson)
    {
        if (string.IsNullOrEmpty(progressionDataJson))
        {
            progressionData = new AchievementProgressionData();
        }
        else
        {
            progressionData = JsonConvert.DeserializeObject<AchievementProgressionData>(progressionDataJson);
        }
        return progressionData;
    }

    protected override string UpdateProgressionStatusText()
    {
        return Utils.SplitStringByUpperCase(progressionData.progressionState.ToString());
    }
}
