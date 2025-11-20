using UnityEngine;

public class GameManager : MonoBehaviour
{
    public BaseHealth baseHealth;
    public GameObject gameOverPanel;

    void Start()
    {
        if (baseHealth != null)
        {
            baseHealth.OnBaseDestroyed += OnGameOver;
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    void OnGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        Time.timeScale = 0f;
        Debug.Log("Game Over");
    }
}
