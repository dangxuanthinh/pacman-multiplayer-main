using DG.Tweening;
using QFSW.QC;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementPanel : MonoBehaviour
{
    [SerializeField] private AchievementUI achievementPrefab;
    [SerializeField] private Transform achievementsHolder;
    [SerializeField] private GameObject backgroundDim;
    [SerializeField] private GameObject panel;
    [SerializeField] private RectTransform achievementCompleteCard;
    [SerializeField] private TextMeshProUGUI achievementCompleteText;
    [SerializeField] private Button closePanelButton;

    public static AchievementPanel Instance;

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
    }

    private void Start()
    {
        closePanelButton.onClick.AddListener(Hide);
        AchievementManager.Instance.OnAchievementComplete += ShowAchievementComplete;
    }

    private void OnDestroy()
    {
        AchievementManager.Instance.OnAchievementComplete -= ShowAchievementComplete;
    }

    [Command]
    public void ShowAchievementCompleteCard()
    {
        achievementCompleteCard.DOAnchorPosX(-260f, 0.75f).OnComplete(() =>
        {
            achievementCompleteCard.DOAnchorPosX(10f, 0.75f).SetDelay(2.5f);
        });
    }

    public void ShowAchievementComplete(AchievementInfo achievementInfo)
    {
        achievementCompleteText.text = $"{achievementInfo.displayName}\n<size=12>Achievement complete!";
        AudioManager.Instance.Play("AchievementComplete", true);
        ShowAchievementCompleteCard();
    }

    public void Open()
    {
        backgroundDim.SetActive(true);
        panel.SetActive(true);
        foreach (Transform child in achievementsHolder)
        {
            Destroy(child.gameObject);
        }
        foreach (Achievement achievement in AchievementManager.Instance.Achievements)
        {
            AchievementInfo achievementInfo = achievement.AchievementInfo;
            AchievementUI achievementUI = Instantiate(achievementPrefab, achievementsHolder);
            achievementUI.SetData(achievementInfo.displayName, achievementInfo.description, achievement.progressionData.progressionStatus);
        }
    }

    public void Hide()
    {
        backgroundDim.SetActive(false);
        panel.SetActive(false);
    }
}
