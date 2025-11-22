using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    public BaseHealth baseHealth;
    public EnemyWaveSpawner waveSpawner;

    public GameObject gameOverPanel;
    public GameObject winPanel;

    [Header("Gem Settings")]
    public int startGems = 100;
    int currentGems;
    public int CurrentGems => currentGems;
    public Action<int> OnGemChanged;

    public enum WinConditionMode
    {
        None,           // 不自动胜利，只能手动触发
        GemAmount,      // 只看宝石 >= gemWinAmount
        SurviveWaves,   // 只看存活波数 >= wavesToSurvive
        GemOrWaves      // 二者任一达成胜利
    }

    public WinConditionMode winMode = WinConditionMode.GemOrWaves;

    [Tooltip("宝石胜利条件")]
    public int gemWinAmount = 1000;

    [Tooltip("需要存活的波数")]
    public int wavesToSurvive = 20;

    bool hasEnded;

    public Action OnGameWon;
    public Action OnGameOver;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        Time.timeScale = 1f;

        if (baseHealth == null)
            baseHealth = FindObjectOfType<BaseHealth>();
        if (baseHealth != null)
            baseHealth.OnBaseDestroyed += HandleBaseDestroyed;

        if (waveSpawner == null)
            waveSpawner = FindObjectOfType<EnemyWaveSpawner>();
        if (waveSpawner != null)
            waveSpawner.OnWavesClearedChanged += HandleWavesCleared;

        currentGems = startGems;
        OnGemChanged?.Invoke(currentGems);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        if (winPanel != null)
            winPanel.SetActive(false);
    }

    void OnDestroy()
    {
        if (baseHealth != null)
            baseHealth.OnBaseDestroyed -= HandleBaseDestroyed;

        if (waveSpawner != null)
            waveSpawner.OnWavesClearedChanged -= HandleWavesCleared;
    }

    //------------------------------------------
    // 失败
    //------------------------------------------
    void HandleBaseDestroyed()
    {
        if (hasEnded) return;
        hasEnded = true;

        Time.timeScale = 0f;
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        OnGameOver?.Invoke();
    }

    //------------------------------------------
    // 波数变化事件
    //------------------------------------------
    void HandleWavesCleared(int cleared)
    {
        if (hasEnded) return;

        CheckAllWinConditions();
    }

    //------------------------------------------
    // 判定胜利（新整合的统一版本）
    //------------------------------------------
    void CheckAllWinConditions()
    {
        if (hasEnded) return;

        switch (winMode)
        {
            case WinConditionMode.None:
                return;

            case WinConditionMode.GemAmount:
                if (currentGems >= gemWinAmount)
                    WinGame();
                break;

            case WinConditionMode.SurviveWaves:
                if (waveSpawner != null && waveSpawner.WavesCleared >= wavesToSurvive)
                    WinGame();
                break;

            case WinConditionMode.GemOrWaves:
                if (currentGems >= gemWinAmount)
                    WinGame();
                else if (waveSpawner != null && waveSpawner.WavesCleared >= wavesToSurvive)
                    WinGame();
                break;
        }
    }

    //------------------------------------------
    // 统一胜利方法
    //------------------------------------------
    void WinGame()
    {
        if (hasEnded) return;
        hasEnded = true;

        Time.timeScale = 0f;

        if (winPanel != null)
            winPanel.SetActive(true);
        else if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        OnGameWon?.Invoke();
    }

    //------------------------------------------
    // 宝石相关
    //------------------------------------------
    public bool HasEnoughGems(int cost)
    {
        if (hasEnded) return false;
        if (cost <= 0) return true;

        return currentGems >= cost;
    }

    public bool SpendGems(int cost)
    {
        if (hasEnded) return false;
        if (cost <= 0) return true;

        if (currentGems < cost)
            return false;

        currentGems -= cost;
        OnGemChanged?.Invoke(currentGems);

        return true;
    }

    public void AddGems(int amount)
    {
        if (hasEnded) return;
        if (amount <= 0) return;

        currentGems += amount;
        OnGemChanged?.Invoke(currentGems);

        CheckAllWinConditions();   // 花钱不会赢，但加钱可能赢
    }
}
