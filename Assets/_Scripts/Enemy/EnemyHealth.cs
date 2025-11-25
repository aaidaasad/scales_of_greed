using UnityEngine;
using System;
using System.Collections;
using Random = UnityEngine.Random;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 10f;
    float currentHealth;

    public Action<float, float> OnHealthChanged;
    public float Current => currentHealth;

    [Header("Damage Text")]
    public bool enableDamageText = true;
    public Color damageTextColor = Color.red;
    public float damageTextSize = 1.0f;
    public float damageTextHeight = 2.0f;
    public float damageTextRandomRadius = 0.3f;

    [Header("Gem Drop")]
    public GameObject gemPickupPrefab;
    [Range(0f, 1f)] public float gemDropChance = 0.3f;
    public int minGemAmount = 1;
    public int maxGemAmount = 3;
    public float gemDropRadius = 0.5f;
    public float gemPopDuration = 0.35f;
    public float gemPopHeight = 1.0f;

    [Header("Dragon Egg Drop")]
    public GameObject dragonEggPrefab;
    [Range(0f, 1f)] public float dragonEggDropChance = 0.1f;
    public float dragonEggDropRadius = 1.0f;
    public float dragonEggPopDuration = 0.35f;
    public float dragonEggPopHeight = 1.0f;
    [Header("Dragon Egg VFX")]
    public GameObject dragonEggSpawnVfx;


    void Awake()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0f) return;

        currentHealth -= amount;
        if (currentHealth < 0f) currentHealth = 0f;

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        ShowDamageText(amount);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void ShowDamageText(float amount)
    {
        if (!enableDamageText) return;
        if (FloatingTextManager.Instance == null) return;

        Vector3 pos = transform.position + Vector3.up * damageTextHeight;

        if (damageTextRandomRadius > 0f)
        {
            Vector2 offset2D = Random.insideUnitCircle * damageTextRandomRadius;
            pos += new Vector3(offset2D.x, 0f, offset2D.y);
        }

        string content = Mathf.RoundToInt(amount).ToString();
        FloatingTextManager.Instance.ShowText(content, pos, damageTextColor, damageTextSize);
    }

    void Die()
    {
        TryDropGem();
        TryDropDragonEgg();
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
            Vector3 start = transform.position + Vector3.up * 0.5f;

            Vector2 offset2D = Random.insideUnitCircle * gemDropRadius;
            Vector3 end = new Vector3(
                start.x + offset2D.x,
                start.y,
                start.z + offset2D.y
            );

            GameObject obj = Instantiate(gemPickupPrefab, start, Quaternion.identity);
            StartCoroutine(PopObject(obj.transform, start, end, gemPopDuration, gemPopHeight));
        }
    }

    void TryDropDragonEgg()
    {
        if (dragonEggPrefab == null) return;
        if (Random.value > dragonEggDropChance) return;

        Vector3 start = transform.position + Vector3.up * 0.5f;

        Vector2 offset2D = Random.insideUnitCircle * dragonEggDropRadius;
        Vector3 end = new Vector3(
            start.x + offset2D.x,
            start.y,
            start.z + offset2D.y
        );

        GameObject eggObj = Instantiate(dragonEggPrefab, start, Quaternion.identity);

        // ⭐⭐⭐ 龙蛋生成特效（新加）
        if (dragonEggSpawnVfx != null)
        {
            GameObject vfx = Instantiate(dragonEggSpawnVfx, start, Quaternion.identity);
            Destroy(vfx, 2f); // 你可以调节持续时间
        }

        StartCoroutine(PopObject(eggObj.transform, start, end, dragonEggPopDuration, dragonEggPopHeight));
    }


    IEnumerator PopObject(Transform obj, Vector3 start, Vector3 end, float duration, float height)
    {
        if (obj == null) yield break;

        if (duration <= 0f)
        {
            obj.position = end;
            yield break;
        }

        float t = 0f;
        while (t < 1f && obj != null)
        {
            t += Time.deltaTime / duration;
            if (t > 1f) t = 1f;

            Vector3 pos = Vector3.Lerp(start, end, t);
            float h = Mathf.Sin(t * Mathf.PI) * height;
            pos.y += h;

            obj.position = pos;
            yield return null;
        }

        if (obj != null)
        {
            obj.position = end;
        }
    }

    public void ResetHealth(float newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}
