using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public class Chest : MonoBehaviour
{
    [Header("Loot Prefabs")]
    public GameObject gemPickupPrefab;
    public GameObject potionPrefab;

    [Header("Loot Amount")]
    public int minGems = 2;
    public int maxGems = 5;
    public int minPotions = 0;
    public int maxPotions = 2;

    [Header("Spawn & Pop Settings")]
    public Transform lootSpawnPoint;
    public float lootDropRadius = 0.8f;
    public float lootPopDuration = 0.25f;
    public float lootPopHeight = 0.6f;

    [Header("Animation")]
    public Animator animator;
    public string openTrigger = "Open";
    public float lootDelay = 1f;   // 开箱后多少秒再弹出奖励

    bool opened;

    public void TryOpen()
    {
        if (opened) return;
        opened = true;

        if (animator != null && !string.IsNullOrEmpty(openTrigger))
        {
            animator.SetTrigger(openTrigger);
        }

        StartCoroutine(OpenRoutine());
    }

    IEnumerator OpenRoutine()
    {
        if (lootDelay > 0f)
        {
            yield return new WaitForSeconds(lootDelay);
        }

        SpawnLoot();
    }

    void SpawnLoot()
    {
        // 生成宝石
        if (gemPickupPrefab != null && maxGems > 0)
        {
            int gemCount = Random.Range(minGems, maxGems + 1);
            for (int i = 0; i < gemCount; i++)
            {
                SpawnOneLoot(gemPickupPrefab);
            }
        }

        // 生成药水
        if (potionPrefab != null && maxPotions > 0)
        {
            int potionCount = Random.Range(minPotions, maxPotions + 1);
            for (int i = 0; i < potionCount; i++)
            {
                SpawnOneLoot(potionPrefab);
            }
        }
    }

    void SpawnOneLoot(GameObject prefab)
    {
        if (prefab == null) return;

        Vector3 start = lootSpawnPoint != null
            ? lootSpawnPoint.position
            : transform.position + Vector3.up * 0.5f;

        Vector2 offset2D = Random.insideUnitCircle.normalized *
                           Random.Range(lootDropRadius * 0.4f, lootDropRadius);

        Vector3 end = new Vector3(
            start.x + offset2D.x,
            start.y,
            start.z + offset2D.y
        );

        GameObject obj = Instantiate(prefab, start, Quaternion.identity);
        StartCoroutine(PopLoot(obj.transform, start, end));
    }

    IEnumerator PopLoot(Transform loot, Vector3 start, Vector3 end)
    {
        if (loot == null) yield break;

        if (lootPopDuration <= 0f)
        {
            loot.position = end;
            yield break;
        }

        float t = 0f;
        while (t < 1f && loot != null)
        {
            t += Time.deltaTime / lootPopDuration;
            if (t > 1f) t = 1f;

            Vector3 pos = Vector3.Lerp(start, end, t);
            float height = Mathf.Sin(t * Mathf.PI) * lootPopHeight;
            pos.y += height;

            loot.position = pos;
            yield return null;
        }

        if (loot != null)
        {
            loot.position = end;
        }
    }
}
