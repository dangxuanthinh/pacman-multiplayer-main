using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class GameScoreManager : NetworkBehaviour
{
    [SerializeField] private int basicPelletScore = 10;
    [SerializeField] private int powerupPelletScore = 50;
    [SerializeField] private int ghostEatenBaseScore;
    [SerializeField] private int ghostEatenConsecutiveBonusScore;

    private float ghostScoreBonusDuration = 10f;

    private NetworkVariable<float> ghostEatenElapsed = new NetworkVariable<float>(0);
    public NetworkVariable<int> CurrentScore = new NetworkVariable<int>(0);

    public UnityAction OnScoreChanged;
    public UnityAction<Vector3, int> OnGhostEatenScoreIncreased;

    public NetworkVariable<int> CurrentGhostEatenScore = new NetworkVariable<int>(0);

    public static GameScoreManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (IsServer)
        {
            GameManager.Instance.OnGhostEaten += OnGhostEaten;
            GameManager.Instance.OnNormalPelletEaten += OnNormalPelletEaten;
            GameManager.Instance.OnPowerupPelletEaten += OnPowerupPelletEaten;
            CurrentGhostEatenScore.Value = ghostEatenBaseScore;
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (IsServer)
        {
            GameManager.Instance.OnGhostEaten -= OnGhostEaten;
            GameManager.Instance.OnNormalPelletEaten -= OnNormalPelletEaten;
            GameManager.Instance.OnPowerupPelletEaten -= OnPowerupPelletEaten;
        }
    }

    private void Update()
    {
        if (!IsServer || GameManager.Instance.gameStarted.Value == false) return;
        HandleGhostEatenScore();
    }

    private void OnGhostEaten(Ghost ghost)
    {
        IncreaseScore(CurrentGhostEatenScore.Value);
        CurrentGhostEatenScore.Value += ghostEatenConsecutiveBonusScore;
        ghostEatenElapsed.Value = 0f;
        OnGhostEatenClientRpc(ghost.transform.position, CurrentGhostEatenScore.Value);
    }

    [ClientRpc]
    private void OnGhostEatenClientRpc(Vector3 ghostPosition, int bonusScoreAmount)
    {
        OnGhostEatenScoreIncreased?.Invoke(ghostPosition, bonusScoreAmount);
    }

    private void OnNormalPelletEaten()
    {
        IncreaseScore(basicPelletScore);
    }

    private void OnPowerupPelletEaten(Vector3 pelletEatenPosition)
    {
        IncreaseScore(powerupPelletScore);
    }

    private void HandleGhostEatenScore()
    {
        if (ghostEatenElapsed != null)
        {
            ghostEatenElapsed.Value += Time.deltaTime;
        }
        if (ghostEatenElapsed.Value >= ghostScoreBonusDuration)
        {
            CurrentGhostEatenScore.Value = ghostEatenBaseScore;
        }
    }

    public void IncreaseScore(int amount)
    {
        CurrentScore.Value += amount;
        IncreaseScoreClientRpc();
    }

    [ClientRpc]
    private void IncreaseScoreClientRpc()
    {
        OnScoreChanged?.Invoke();
    }

    public int GetCoinReward()
    {
        int amount = Mathf.FloorToInt(CurrentScore.Value / 100f);
        MoneyManager.Instance.currentCoin += amount;
        return amount;
    }
}
