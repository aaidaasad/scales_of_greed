using UnityEngine;
using UnityEngine.UI;

public class BaseHealthBar : MonoBehaviour
{
    public BaseHealth baseHealth;
    public Image fillImage;

    void Start()
    {
        if (baseHealth != null)
        {
            baseHealth.OnHealthChanged += UpdateBar;
            // 初始化一次
            UpdateBar(baseHealth.Current, baseHealth.Max);
        }
    }

    void UpdateBar(float current, float max)
    {
        float f = max > 0 ? current / max : 0f;
        fillImage.fillAmount = f;
    }
}
