using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Random = UnityEngine.Random;

public class AlchemyStation : MonoBehaviour
{
    [Header("Spawn")]
    public Transform spawnPoint;

    [Header("Progress Bar")]
    public Transform progressBarRoot;
    public Image progressFill;

    [Header("Potion Drop")]
    public float potionDropRadius = 2f;
    public float potionPopHeight = 1.5f;
    public float potionPopDuration = 0.4f;

    [Header("Gem Consume FX")]
    public GameObject gemVisualPrefab;   // 用作演出的宝石预制体（建议用你现有的 gem 模型复制一份）
    public Transform gemTargetPoint;     // 宝石飞向的目标点（一般是炉口位置）
    public int gemPerVisual = 10;        // 每多少真实 gem 表现为 1 颗宝石
    public float gemFlyHeight = 2f;
    public float gemFlyDuration = 0.4f;
    public float gemSpawnOffsetY = 1.2f;

    Transform currentPlayer;

    GameObject currentPotionPrefab;
    float craftTime;
    float craftTimer;
    bool isCrafting;

    Camera cam;

    public bool IsCrafting => isCrafting;

    void Awake()
    {
        cam = Camera.main;

        if (progressBarRoot != null)
            progressBarRoot.gameObject.SetActive(false);

        if (progressFill != null)
            progressFill.fillAmount = 0f;
    }

    void Update()
    {
        if (isCrafting)
        {
            craftTimer += Time.deltaTime;
            float t = craftTime > 0f ? craftTimer / craftTime : 1f;
            t = Mathf.Clamp01(t);

            if (progressFill != null)
                progressFill.fillAmount = t;

            if (craftTimer >= craftTime)
            {
                CompleteCraft();
            }
        }

        if (progressBarRoot != null && cam != null && progressBarRoot.gameObject.activeSelf)
        {
            Vector3 dir = cam.transform.position - progressBarRoot.position;
            progressBarRoot.rotation = Quaternion.LookRotation(-dir, Vector3.up);
        }
    }

    public void StartCraft(GameObject potionPrefab, float time, int gemCost)
    {
        if (isCrafting) return;

        currentPotionPrefab = potionPrefab;
        craftTime = Mathf.Max(0.01f, time);
        craftTimer = 0f;
        isCrafting = true;

        if (progressBarRoot != null)
            progressBarRoot.gameObject.SetActive(true);

        if (progressFill != null)
            progressFill.fillAmount = 0f;

        // 🔥 开始播放宝石飞向炼金炉的演出
        PlayGemConsumeFx(gemCost);
    }

    void PlayGemConsumeFx(int gemCost)
    {
        if (gemVisualPrefab == null) return;
        if (currentPlayer == null) return;

        Transform target = gemTargetPoint != null
            ? gemTargetPoint
            : (spawnPoint != null ? spawnPoint : transform);

        // 至少一颗，gemPerVisual 控制“一颗代表多少真实gem”
        int visualCount = Mathf.Max(1, gemCost / Mathf.Max(1, gemPerVisual));
        visualCount = Mathf.Min(visualCount, 6); // 防止太多，随便限制个上限

        Vector3 playerPos = currentPlayer.position + Vector3.up * gemSpawnOffsetY;

        for (int i = 0; i < visualCount; i++)
        {
            // 稍微错开生成位置（从玩家身上散开一点）
            Vector3 start = playerPos;
            start += new Vector3(
                Random.Range(-0.4f, 0.4f),
                Random.Range(-0.2f, 0.2f),
                Random.Range(-0.4f, 0.4f)
            );

            GameObject gemObj = Instantiate(gemVisualPrefab, start, Quaternion.identity);

            // 确保不会被捡起（如果你不小心用的是可拾取的 gem prefab）
            var pickup = gemObj.GetComponent<GemPickup>();
            if (pickup != null) Destroy(pickup);
            var col = gemObj.GetComponent<Collider>();
            if (col != null) col.enabled = false;
            var rb = gemObj.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            float delay = Random.Range(0f, 0.15f);
            StartCoroutine(GemFlyRoutine(gemObj.transform, start, target.position, delay));
        }
    }

    System.Collections.IEnumerator GemFlyRoutine(Transform gem, Vector3 start, Vector3 end, float delay)
    {
        if (gem == null) yield break;

        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        float t = 0f;
        float duration = Mathf.Max(0.01f, gemFlyDuration);

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float clamped = Mathf.Clamp01(t);

            // 抛物线插值
            Vector3 pos = Vector3.Lerp(start, end, clamped);
            float height = Mathf.Sin(clamped * Mathf.PI) * gemFlyHeight;
            pos.y += height;

            if (gem != null)
                gem.position = pos;

            yield return null;
        }

        if (gem != null)
            Destroy(gem.gameObject);
    }

    void CompleteCraft()
    {
        isCrafting = false;

        if (progressFill != null)
            progressFill.fillAmount = 1f;

        Transform spawn = spawnPoint != null ? spawnPoint : transform;

        if (currentPotionPrefab != null)
        {
            Vector3 start = spawn.position + Vector3.up * 0.5f;

            Vector2 offset2D = Random.insideUnitCircle.normalized *
                               Random.Range(potionDropRadius * 0.4f, potionDropRadius);

            Vector3 end = new Vector3(
                spawn.position.x + offset2D.x,
                spawn.position.y,
                spawn.position.z + offset2D.y
            );

            GameObject obj = Instantiate(currentPotionPrefab, start, Quaternion.identity);
            StartCoroutine(PopPotion(obj.transform, start, end));
        }

        if (progressBarRoot != null)
            progressBarRoot.gameObject.SetActive(false);

        currentPotionPrefab = null;
        craftTime = 0f;
        craftTimer = 0f;
    }

    IEnumerator PopPotion(Transform potion, Vector3 start, Vector3 end)
    {
        if (potion == null) yield break;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, potionPopDuration);
            float clamped = Mathf.Clamp01(t);

            Vector3 pos = Vector3.Lerp(start, end, clamped);
            float height = Mathf.Sin(clamped * Mathf.PI) * potionPopHeight;
            pos.y += height;

            potion.position = pos;
            yield return null;
        }

        if (potion != null)
        {
            potion.position = end;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            currentPlayer = player.transform;

            if (PotionCraftingUIManager.Instance != null)
            {
                PotionCraftingUIManager.Instance.OpenForStation(this);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            if (PotionCraftingUIManager.Instance != null &&
                PotionCraftingUIManager.Instance.CurrentStation == this)
            {
                PotionCraftingUIManager.Instance.Close();
            }

            if (currentPlayer == player.transform)
            {
                currentPlayer = null;
            }
        }
    }
}
