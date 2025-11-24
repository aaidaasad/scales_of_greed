using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GemUI : MonoBehaviour
{
    public TextMeshProUGUI gemText;

    void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGemChanged += OnGemChanged;
            OnGemChanged(GameManager.Instance.CurrentGems);
        }
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGemChanged -= OnGemChanged;
        }
    }

    void OnGemChanged(int value)
    {
        if (gemText != null)
        {
            gemText.text = value.ToString();

        }
    }
}
