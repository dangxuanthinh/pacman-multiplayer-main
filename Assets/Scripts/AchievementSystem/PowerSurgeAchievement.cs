using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Collect your first power-up
public class PowerSurgeAchievement : Achievement
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
                GameManager.Instance.OnGameStart += OnGameStart;
            }
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (Pacman.LocalInstance != null)
        {
            Pacman.LocalInstance.OnPowerupPickedUp -= OnPowerupPickedUp;
        }
    }

    private void OnGameStart()
    {
        Pacman.LocalInstance.OnPowerupPickedUp += OnPowerupPickedUp;
    }

    private void OnPowerupPickedUp(PowerUp powerup)
    {
        if (progressionData.progressionState == AchievementState.Finished) return;
        UpdateProgressionData(progressionData);
        FinishAchievement();
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
