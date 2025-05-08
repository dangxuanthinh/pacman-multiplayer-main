using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Achievement : MonoBehaviour
{
    public AchievementProgressionData progressionData;

    public AchievementInfo AchievementInfo { get; private set; }

    public void InitializeAchievement(AchievementInfo achievementInfo, string progressionDataJson)
    {
        this.AchievementInfo = achievementInfo;
        progressionData = InitializeProgressionData(progressionDataJson);
        progressionData.progressionStatus = UpdateProgressionStatusText();
        if (progressionData.progressionState == AchievementState.Finished)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }

    // Function to parse data for each achievement
    protected abstract AchievementProgressionData InitializeProgressionData(string progressionDataJson);

    // Returns a string representing the status of the achievement
    protected abstract string UpdateProgressionStatusText();

    protected void FinishAchievement()
    {
        if (progressionData.progressionState == AchievementState.Finished) return;
        progressionData.progressionState = AchievementState.Finished;
        progressionData.progressionStatus = "Finished";
        AchievementManager.Instance.CompleteAchievement(this);
    }

    protected void UpdateProgressionData(AchievementProgressionData progressionData)
    {
        this.progressionData = progressionData;
        this.progressionData.progressionStatus = UpdateProgressionStatusText();
    }
}
