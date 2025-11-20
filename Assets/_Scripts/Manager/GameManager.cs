using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public BaseHealth baseHealth;
    public GameObject gameOverPanel;

    public int startGems = 100;

    int currentGems;

    public int CurrentGems => currentGems;

    public Action<int> OnGemChanged;

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

        if (baseHealth != null)
        {
            baseHealth.OnBaseDestroyed += OnGameOver;
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        currentGems = startGems;
        OnGemChanged?.Invoke(currentGems);
    }

    void OnGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        Time.timeScale = 0f;
    }

    public bool HasEnoughGems(int cost)
    {
        if (cost <= 0) return true;
        return currentGems >= cost;
    }

    public bool SpendGems(int cost)
    {
        if (!HasEnoughGems(cost)) return false;

        currentGems -= cost;
        OnGemChanged?.Invoke(currentGems);

        return true;
    }

    public void AddGems(int amount)
    {
        if (amount <= 0) return;

        currentGems += amount;
        OnGemChanged?.Invoke(currentGems);
    }
}
