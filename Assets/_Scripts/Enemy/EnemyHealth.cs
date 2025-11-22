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

    [Header("Gem Drop")]
    public GameObject gemPickupPrefab;
    [Range(0f, 1f)] public float gemDropChance = 0.3f;
    public int minGemAmount = 1;
    public int maxGemAmount = 3;
    public float gemDropRadius = 0.5f;

    [Header("Dragon Egg Drop")]
    public GameObject dragonEggPrefab;
    [Range(0f, 1f)] public float dragonEggDropChance = 0.1f;
    public float dragonEggDropRadius = 1.0f;

    // 类似 OreNode 的 Pop 动画参数
    public float dragonEggPopDuration = 0.35f;
    public float dragonEggPopHeight = 1.0f;

    // 如果不指定，就用敌人自身位置+Y
    public Transform dragonEggSpawnPoint;

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
            // 简单的随机半径散落
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

    void TryDropDragonEgg()
    {
        if (dragonEggPrefab == null) return;
        if (Random.value > dragonEggDropChance) return;

        // 起点：敌人身上的挂点，否则用自身位置往上抬一点
        Vector3 start = dragonEggSpawnPoint != null
            ? dragonEggSpawnPoint.position
            : transform.position + Vector3.up * 1.0f;

        // 目标位置：水平随机一个方向 + 半径，落点稍微偏移
        Vector2 offset2D = Random.insideUnitCircle.normalized *
                           Random.Range(dragonEggDropRadius * 0.4f, dragonEggDropRadius);

        Vector3 end = new Vector3(
            start.x + offset2D.x,
            start.y,
            start.z + offset2D.y
        );

        GameObject eggObj = Instantiate(dragonEggPrefab, start, Quaternion.identity);

        // 用协程做类似 OreNode 的 Pop 抛物线动画
        StartCoroutine(PopDragonEgg(eggObj.transform, start, end));
    }

    IEnumerator PopDragonEgg(Transform egg, Vector3 start, Vector3 end)
    {
        if (egg == null) yield break;

        if (dragonEggPopDuration <= 0f)
        {
            egg.position = end;
            yield break;
        }

        float t = 0f;
        while (t < 1f && egg != null)
        {
            t += Time.deltaTime / dragonEggPopDuration;
            if (t > 1f) t = 1f;

            // 线性插值从 start 到 end
            Vector3 pos = Vector3.Lerp(start, end, t);

            // 用一个简单的 sin 曲线做中间“弹起”的高度（类似 OreNode 的 PopGem）
            float height = Mathf.Sin(t * Mathf.PI) * dragonEggPopHeight;
            pos.y += height;

            egg.position = pos;

            yield return null;
        }

        if (egg != null)
        {
            egg.position = end;
        }
    }

    public void ResetHealth(float newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}
