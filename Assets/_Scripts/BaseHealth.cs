using UnityEngine;
using System;

public class BaseHealth : MonoBehaviour
{
    public float maxHealth = 20f;

    float currentHealth;

    public Action<float, float> OnHealthChanged;  // 当前血量, 最大血量
    public Action OnBaseDestroyed;               // 基地被摧毁（Game Over）

    public float Current => currentHealth;
    public float Max => maxHealth;

    void Awake()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0f) currentHealth = 0f;

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0f)
        {
            OnBaseDestroyed?.Invoke();
        }
    }
}
