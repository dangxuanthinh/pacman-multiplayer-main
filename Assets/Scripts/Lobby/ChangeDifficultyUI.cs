using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChangeDifficultyUI : MonoBehaviour
{
    [SerializeField] private Button forwardButton;
    [SerializeField] private Button previousButton;
    [SerializeField] private TextMeshProUGUI currentDifficultyText;
    private int currentDifficultyIndex;

    private void Awake()
    {
        forwardButton.onClick.AddListener(() =>
        {
            NextDifficulty();
        });
        previousButton.onClick.AddListener(() =>
        {
            PreviousDifficulty();
        });
    }

    private void NextDifficulty()
    {
        currentDifficultyIndex = (currentDifficultyIndex + 1) % GameResourceHandler.Instance.difficulties.Count;
        SetDifficultyText();
    }

    private void PreviousDifficulty()
    {
        currentDifficultyIndex--;
        if (currentDifficultyIndex < 0)
        {
            currentDifficultyIndex = GameResourceHandler.Instance.difficulties.Count - 1;
        }
        SetDifficultyText();
    }

    private void SetDifficultyText()
    {
        currentDifficultyText.text = GameResourceHandler.Instance.difficulties[currentDifficultyIndex].difficultyName.ToUpper();
    }

    public GhostDifficulty GetSelectedDifficulty()
    {
        return GameResourceHandler.Instance.difficulties[currentDifficultyIndex];
    }

    public void ResetToDefault()
    {
        currentDifficultyIndex = 0;
        SetDifficultyText();
    }

    public void SetSelectedDifficulty(GhostDifficulty ghostDifficulty)
    {
        currentDifficultyIndex = GameResourceHandler.Instance.difficulties.IndexOf(ghostDifficulty);
        SetDifficultyText();
    }
}
