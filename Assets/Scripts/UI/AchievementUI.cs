using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AchievementUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI achievementName;
    [SerializeField] private TextMeshProUGUI achievementDescription;
    [SerializeField] private TextMeshProUGUI achievementStatus;

    public void SetData(string name, string description, string status)
    {
        achievementName.text = name;
        achievementDescription.text = description;
        achievementStatus.text = status;
        if (status == "Finished")
        {
            achievementStatus.color = Color.green;
        }
        else
        {
            achievementStatus.color = Color.yellow;
        }
    }
}
