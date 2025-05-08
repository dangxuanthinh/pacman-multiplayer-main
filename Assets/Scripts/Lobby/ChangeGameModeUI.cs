using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChangeGameModeUI : MonoBehaviour
{
    [SerializeField] private Button forwardButton;
    [SerializeField] private Button previousButton;
    [SerializeField] private TextMeshProUGUI currentGameModeText;
    [SerializeField] private TextMeshProUGUI description;

    [SerializeField] private List<GameModeSelect> gameModes = new List<GameModeSelect>();
    private int currentGameModeIndex;

    private void Awake()
    {
        forwardButton.onClick.AddListener(() =>
        {
            NextGameMode();
        });
        previousButton.onClick.AddListener(() =>
        {
            PreviousGameMode();
        });
    }

    private void NextGameMode()
    {
        currentGameModeIndex = (currentGameModeIndex + 1) % gameModes.Count;
        SetDescription();
    }

    private void PreviousGameMode()
    {
        currentGameModeIndex--;
        if (currentGameModeIndex < 0)
        {
            currentGameModeIndex = gameModes.Count - 1;
        }
        SetDescription();
    }

    private void SetDescription()
    {
        currentGameModeText.text = gameModes[currentGameModeIndex].gameMode.ToString();
        description.text = gameModes[currentGameModeIndex].description;
    }

    public GameMode GetSelectedGameMode()
    {
        return gameModes[currentGameModeIndex].gameMode;
    }

    public void ResetToDefault()
    {
        currentGameModeIndex = 0;
        SetDescription();
    }

    public void SetSelectedGameMode(GameMode gameMode)
    {
        currentGameModeIndex = gameModes.IndexOf(gameModes.FirstOrDefault(m => m.gameMode == gameMode));
        SetDescription();
    }

    [Serializable]
    private class GameModeSelect
    {
        public GameMode gameMode;
        [TextArea(2, 3)]
        public string description;
    }
}
