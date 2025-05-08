using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Win without eating a ghost
public class PacifistRunAchievement : Achievement
{
    private bool ghostEaten;

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
                ghostEaten = false;
                GameManager.Instance.OnGameVictory += OnVictory;
                GameManager.Instance.OnGhostEatenByLocalPlayer += OnGhostEaten;
            }
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameVictory -= OnVictory;
            GameManager.Instance.OnGhostEatenByLocalPlayer -= OnGhostEaten;
        }
    }

    private void OnGhostEaten()
    {
        ghostEaten = true;
    }

    private void OnVictory()
    {
        if (ghostEaten == false)
        {
            UpdateProgressionData(progressionData);
            FinishAchievement();
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
