using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Be the last player standing in a multiplayer game
public class LastPacStandingAchievement : Achievement
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
                GameManager.Instance.OnPacmanLivesChanged += OnPlayerLivesChanged;
            }
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPacmanLivesChanged -= OnPlayerLivesChanged;
        }
    }

    private void OnPlayerLivesChanged()
    {
        if (progressionData.progressionState == AchievementState.Finished) return;

        bool localPlayerAlive = false;
        bool atLeastOneOtherPlayerAlive = false;

        if (GameManager.Instance.playerRemainingLivesTable.Count <= 1) return;

        foreach (var kvp in GameManager.Instance.playerRemainingLivesTable)
        {
            ulong clientID = kvp.Key;
            int remainingLives = kvp.Value;

            if (clientID == Pacman.LocalInstance.OwnerClientId && remainingLives > 0)
            {
                localPlayerAlive = true;
                continue;
            }

            if (remainingLives > 0)
            {
                atLeastOneOtherPlayerAlive = true;
            }
        }

        if (localPlayerAlive && atLeastOneOtherPlayerAlive == false)
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
