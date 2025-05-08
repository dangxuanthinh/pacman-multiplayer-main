using Newtonsoft.Json;
using QFSW.QC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SocialPlatforms.Impl;

public class AchievementManager : MonoBehaviour, IDataPersistence
{
    private Dictionary<string, AchievementInfo> achievementDictionary = new Dictionary<string, AchievementInfo>();
    private Dictionary<string, string> achievementProgressionDictionary;
    private List<Achievement> spawnedAchievements = new List<Achievement>();

    public UnityAction<AchievementInfo> OnAchievementComplete;
    public UnityAction OnAchievementRefreshed;

    public static AchievementManager Instance;

    public List<Achievement> Achievements => spawnedAchievements;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        LoadAchievements();
    }

    private void Start()
    {
        SpawnAchievementObjects();
    }

    private void LoadAchievements()
    {
        AchievementInfo[] achievements = Resources.LoadAll<AchievementInfo>("Achievements");
        foreach (AchievementInfo achievement in achievements)
        {
            achievementDictionary[achievement.id] = achievement;
        }
    }

    private void SpawnAchievementObjects()
    {
        foreach (var kvp in achievementDictionary)
        {
            string achievementID = kvp.Key;
            AchievementInfo achievementInfo = kvp.Value;
            Achievement achievement = Instantiate(achievementInfo.achievementPrefab, transform);
            spawnedAchievements.Add(achievement);
            achievement.InitializeAchievement(achievementInfo, GetAchievementProgressionJson(achievementID));
        }
        OnAchievementRefreshed?.Invoke();
    }

    private string GetAchievementProgressionJson(string id)
    {
        if (achievementProgressionDictionary.TryGetValue(id, out string value))
        {
            return value;
        }
        else
        {
            achievementProgressionDictionary[id] = string.Empty;
            return achievementProgressionDictionary[id];
        }
    }

    private AchievementInfo GetAchievementByID(string id)
    {
        return achievementDictionary[id];
    }

    [Command]
    public string GetAchievementStatus(string id)
    {
        return spawnedAchievements.Find(a => a.AchievementInfo.id == id).progressionData.progressionStatus;
    }

    public void CompleteAchievement(Achievement achievement)
    {
        Debug.Log("Achievement " + achievement.AchievementInfo.displayName + " completed!");
        OnAchievementComplete?.Invoke(achievement.AchievementInfo);
    }

    public void LoadData(GameData data)
    {
        achievementProgressionDictionary = data.achievementProgressionDictionary;
        if (achievementProgressionDictionary == null || achievementProgressionDictionary.Count == 0)
        {
            foreach (var kvp in achievementDictionary)
            {
                string achievementID = kvp.Key;
                GetAchievementProgressionJson(achievementID);
            }
        }
        foreach (var achievement in spawnedAchievements)
        {
            achievement.InitializeAchievement(achievement.AchievementInfo, achievementProgressionDictionary[achievement.AchievementInfo.id]);
        }
    }

    public void SaveData(GameData data)
    {
        foreach (Achievement achievement in spawnedAchievements)
        {
            if (achievement != null)
            {
                achievementProgressionDictionary[achievement.AchievementInfo.id] = JsonConvert.SerializeObject(achievement.progressionData);
                //Debug.Log(achievement.AchievementInfo.id + ": " + achievementProgressionDictionary[achievement.AchievementInfo.id]);
            }
        }
        data.achievementProgressionDictionary = achievementProgressionDictionary;
    }
}
