using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PacmanLevelSelector : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Image levelPreview;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button previousLevelButton;
    private int currentLevelIndex;

    private void Start()
    {
        nextLevelButton.onClick.AddListener(NextLevel);
        previousLevelButton.onClick.AddListener(PreviousLevel);
    }

    public void NextLevel()
    {
        currentLevelIndex = (currentLevelIndex + 1) % GameResourceHandler.Instance.maps.Count;
        ShowLevel();
    }

    public void PreviousLevel()
    {
        currentLevelIndex--;
        if (currentLevelIndex < 0)
        {
            currentLevelIndex = GameResourceHandler.Instance.maps.Count - 1;
        }
        ShowLevel();
    }

    public void ShowLevel()
    {
        levelPreview.sprite = GameResourceHandler.Instance.maps[currentLevelIndex].previewSprite;
        levelPreview.preserveAspect = true;
        levelText.text = "LEVEL " + (currentLevelIndex + 1).ToString();
    }

    public PacmanMap GetSelectedMap()
    {
        return GameResourceHandler.Instance.maps[currentLevelIndex];
    }

    public void SetSelectedMap(PacmanMap map)
    {
        currentLevelIndex = GameResourceHandler.Instance.maps.IndexOf(map);
        ShowLevel();
    }

    public void ResetToDefault()
    {
        currentLevelIndex = 0;
        ShowLevel();
    }
}
