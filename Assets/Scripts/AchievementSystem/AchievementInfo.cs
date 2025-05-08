using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AchievementState
{
    InProgress,
    Finished
}

[CreateAssetMenu(fileName = "Achievement Info", menuName = "Achievement Info")]
public class AchievementInfo : ScriptableObject
{
    public string id;
    public string displayName;
    [TextArea(1, 2)]
    public string description;
    public Achievement achievementPrefab;
}
