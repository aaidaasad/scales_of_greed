using UnityEngine;
using System.Collections;

public class WaveRewardChest : MonoBehaviour
{
    public GameObject gemPickupPrefab;
    public Transform gemSpawnPoint;

    public float gemDropRadius = 1.5f;
    public float gemPopHeight = 0.75f;
    public float gemPopDuration = 0.25f;

    public int baseReward = 10;
    public int rewardPerWave = 5;
    public int valuePerPickup = 5;

    [Header("Chest Anim")]
    public Animator chestAnimator;
    public string openTriggerName = "Open";
    public string closeTriggerName = "Close";   // 新增：关闭 trigger
    public float openAnimDelay = 1f;            // 箱子开到“完全打开”的时间
    public float stayOpenTime = 0.5f;           // 打开后保持多久再关
    public float closeAnimDelay = 0.5f;         // 关箱动画时长（可选）

    EnemyWaveSpawner waveSpawner;

    bool isPlayingSequence = false;             // 防止重复触发

    void Start()
    {
        if (GameManager.Instance != null)
            waveSpawner = GameManager.Instance.waveSpawner;

        if (waveSpawner == null)
            waveSpawner = FindObjectOfType<EnemyWaveSpawner>();

        if (waveSpawner != null)
            waveSpawner.OnWavesClearedChanged += HandleWaveCleared;
    }

    void OnDestroy()
    {
        if (waveSpawner != null)
            waveSpawner.OnWavesClearedChanged -= HandleWaveCleared;
    }

    void HandleWaveCleared(int cleared)
    {
        int reward = Mathf.Max(0, baseReward + rewardPerWave * cleared);
        if (reward <= 0) return;

        StartCoroutine(PlayRewardSequence(reward));
    }

    IEnumerator PlayRewardSequence(int totalReward)
    {
        if (isPlayingSequence)
            yield break; // 正在播就不要重入

        isPlayingSequence = true;

        // 1. 播放开箱
        if (chestAnimator != null && !string.IsNullOrEmpty(openTriggerName))
        {
            chestAnimator.SetTrigger(openTriggerName);
        }

        if (openAnimDelay > 0f)
            yield return new WaitForSeconds(openAnimDelay);

        // 2. 生成并弹出宝石
        SpawnRewardGems(totalReward);

        // 等宝石弹起动画大概结束（可以按需求调整）
        float popTotal = gemPopDuration + 0.1f;
        yield return new WaitForSeconds(popTotal + stayOpenTime);

        // 3. 播放关箱
        if (chestAnimator != null && !string.IsNullOrEmpty(closeTriggerName))
        {
            chestAnimator.SetTrigger(closeTriggerName);
        }

        if (closeAnimDelay > 0f)
            yield return new WaitForSeconds(closeAnimDelay);

        isPlayingSequence = false;   // 回到可再次触发的状态
    }

    void SpawnRewardGems(int totalReward)
    {
        if (gemPickupPrefab == null) return;

        int perPickup = Mathf.Max(1, valuePerPickup);
        int gemCount = Mathf.CeilToInt(totalReward / (float)perPickup);

        for (int i = 0; i < gemCount; i++)
        {
            Vector3 start = gemSpawnPoint != null
                ? gemSpawnPoint.position
                : transform.position + Vector3.up * 0.5f;

            Vector2 offset2D = Random.insideUnitCircle.normalized *
                               Random.Range(gemDropRadius * 0.4f, gemDropRadius);

            Vector3 end = new Vector3(
                start.x + offset2D.x,
                start.y,
                start.z + offset2D.y
            );

            GameObject obj = Instantiate(gemPickupPrefab, start, Quaternion.identity);

            GemPickup pickup = obj.GetComponent<GemPickup>();
            if (pickup != null)
            {
                pickup.amount = perPickup;
            }

            StartCoroutine(PopGem(obj.transform, start, end));
        }
    }

    IEnumerator PopGem(Transform gem, Vector3 start, Vector3 end)
    {
        if (gem == null) yield break;

        if (gemPopDuration <= 0f)
        {
            gem.position = end;
            yield break;
        }

        float t = 0f;
        while (t < 1f && gem != null)
        {
            t += Time.deltaTime / gemPopDuration;
            if (t > 1f) t = 1f;

            Vector3 pos = Vector3.Lerp(start, end, t);
            float height = Mathf.Sin(t * Mathf.PI) * gemPopHeight;
            pos.y += height;

            gem.position = pos;
            yield return null;
        }
    }
}
