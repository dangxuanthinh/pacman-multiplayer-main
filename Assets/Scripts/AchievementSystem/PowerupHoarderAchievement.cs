using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class PowerupHoarderProgressionData : AchievementProgressionData
{
    public int powerupEaten = 0;
}

// Collect a total of 10 power-up in a single match
public class PowerupHoarderAchievement : Achievement
{
    private PowerupHoarderProgressionData powerupHoarderProgressionData;
    public int totalPowerupsToComplete = 10;

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
                GameManager.Instance.OnGameLose += ResetProgress;
                GameManager.Instance.OnGameVictory += ResetProgress;
            }
        }
        else
        {
            ResetProgress();
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

    private void OnApplicationQuit()
    {
        ResetProgress();
    }

    private void OnGameStart()
    {
        Pacman.LocalInstance.OnPowerupPickedUp += OnPowerupPickedUp;
    }

    private void ResetProgress()
    {
        if (progressionData.progressionState == AchievementState.Finished) return;
        powerupHoarderProgressionData.powerupEaten = 0;
        UpdateProgressionData(powerupHoarderProgressionData);
    }

    private void OnPowerupPickedUp(PowerUp powerup)
    {
        if (progressionData.progressionState == AchievementState.Finished) return;
        if (powerupHoarderProgressionData.powerupEaten < totalPowerupsToComplete)
        {
            powerupHoarderProgressionData.powerupEaten++;
            UpdateProgressionData(powerupHoarderProgressionData);
        }
        if (powerupHoarderProgressionData.powerupEaten >= totalPowerupsToComplete)
        {
            FinishAchievement();
        }
    }

    protected override AchievementProgressionData InitializeProgressionData(string progressionDataJson)
    {
        if (string.IsNullOrEmpty(progressionDataJson))
        {
            powerupHoarderProgressionData = new PowerupHoarderProgressionData();
        }
        else
        {
            powerupHoarderProgressionData = JsonConvert.DeserializeObject<PowerupHoarderProgressionData>(progressionDataJson);
        }
        return powerupHoarderProgressionData;
    }

    protected override string UpdateProgressionStatusText()
    {
        return Utils.SplitStringByUpperCase(progressionData.progressionState.ToString());

    }
}
