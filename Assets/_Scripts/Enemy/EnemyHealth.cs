using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 10f;
    float currentHealth;

    public Action<float, float> OnHealthChanged;

    public GameObject gemPickupPrefab;
    [Range(0f, 1f)] public float gemDropChance = 0.3f;
    public int minGemAmount = 1;
    public int maxGemAmount = 3;
    public float gemDropRadius = 0.5f;

    public float Current => currentHealth;   // 新增

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
            Die();
        }
    }

    void Die()
    {
        TryDropGem();
        Destroy(gameObject);
    }

    void TryDropGem()
    {
        if (gemPickupPrefab == null) return;
        if (Random.value > gemDropChance) return;

        int totalGems = Random.Range(minGemAmount, maxGemAmount + 1);
        if (totalGems <= 0) return;

        for (int i = 0; i < totalGems; i++)
        {
            Vector2 offset2D = Random.insideUnitCircle * gemDropRadius;
            Vector3 pos = transform.position + new Vector3(offset2D.x, 0f, offset2D.y);

            GameObject obj = Instantiate(gemPickupPrefab, pos, Quaternion.identity);

            GemPickup pickup = obj.GetComponent<GemPickup>();
            if (pickup != null)
            {
                pickup.amount = 1;
            }
        }
    }

    public void ResetHealth(float newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}
