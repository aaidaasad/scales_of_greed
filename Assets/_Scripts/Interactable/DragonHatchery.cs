using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DragonHatchery : MonoBehaviour
{
    [Header("Hatch Settings")]
    public float hatchTime = 5f;
    public int hatchGemCost = 30;

    [Header("UI")]
    public Image progressFill;
    public Transform progressBarRoot;

    [Header("Sockets")]
    public Transform eggSocket;
    public Transform dragonSpawnPoint;

    [Header("Result")]
    public GameObject littleDragonPrefab;

    [Header("Debug")]
    public bool logEvents = true;

    [Header("Feedback")]
    public float notEnoughTextOffsetY = 2f;
    public float spentTextOffsetY = 2.5f;
    public float hatchDoneTextOffsetY = 2f;

    [Header("Egg Move")]
    public float eggMoveDuration = 0.4f;
    public float eggMoveHeight = 1.5f;

    [Header("Gem Charge FX")]
    public GameObject gemChargePrefab;   // 用来做演出的宝石模型（不能拾取）
    public float gemRingRadius = 2f;
    public float gemRingHeight = 0.5f;
    public float gemFlyHeight = 2f;
    public float gemFlyDuration = 0.4f;
    public float gemDelayBetween = 0.08f;
    public int gemVisualPerCost = 10;    // 每多少真实宝石显示 1 颗
    public int minGemChargeCount = 6;
    public int maxGemChargeCount = 16;

    [Header("Gem Charge Source")]
    public Transform gemChargeSource;    // 一般拖玩家
    public float gemSourceOffsetY = 1.5f;
    public float gemRingAppearDuration = 0.3f;


    public float CurrentProgress { get; private set; }

    Egg currentEgg;
    Egg pendingEggForCost;
    bool isHatching;
    bool hasShownNotEnoughForCurrentEgg;
    Camera cam;

    void Awake()
    {
        cam = Camera.main;

        CurrentProgress = 0f;

        if (progressFill != null)
            progressFill.fillAmount = 0f;

        if (progressBarRoot != null)
            progressBarRoot.gameObject.SetActive(false);
    }

    void Update()
    {
        if (progressBarRoot != null && cam != null)
        {
            Vector3 dir = progressBarRoot.position - cam.transform.position;
            if (dir.sqrMagnitude > 0.001f)
                progressBarRoot.rotation = Quaternion.LookRotation(dir);
        }

        if (progressFill != null)
            progressFill.fillAmount = CurrentProgress;
    }

    void OnTriggerEnter(Collider other)
    {
        if (logEvents) Debug.Log($"[Hatchery] OnTriggerEnter with {other.name}");
        TryStartWith(other);
    }

    void OnTriggerStay(Collider other)
    {
        if (!isHatching && currentEgg == null)
            TryStartWith(other);
    }

    void OnTriggerExit(Collider other)
    {
        Egg egg = other.GetComponentInParent<Egg>();
        if (egg != null && egg == pendingEggForCost)
        {
            pendingEggForCost = null;
            hasShownNotEnoughForCurrentEgg = false;
        }
    }

    void TryStartWith(Collider other)
    {
        if (isHatching || currentEgg != null)
            return;

        Egg egg = other.GetComponentInParent<Egg>();
        if (egg == null)
        {
            if (logEvents) Debug.Log($"[Hatchery] Object {other.name} is not an Egg.");
            return;
        }

        if (pendingEggForCost != egg)
        {
            pendingEggForCost = egg;
            hasShownNotEnoughForCurrentEgg = false;
        }

        if (hatchGemCost > 0 && GameManager.Instance != null)
        {
            if (!GameManager.Instance.HasEnoughGems(hatchGemCost))
            {
                if (logEvents)
                    Debug.Log($"[Hatchery] Not enough gems to hatch. Need {hatchGemCost}, current {GameManager.Instance.CurrentGems}");

                if (!hasShownNotEnoughForCurrentEgg && FloatingTextManager.Instance != null)
                {
                    hasShownNotEnoughForCurrentEgg = true;
                    Vector3 pos = transform.position + Vector3.up * (notEnoughTextOffsetY + 2f);
                    FloatingTextManager.Instance.ShowText("Gem Not Enough...", pos, Color.red, 1.2f);
                }

                return;
            }

            if (!GameManager.Instance.SpendGems(hatchGemCost))
            {
                if (logEvents) Debug.Log($"[Hatchery] SpendGems failed.");
                return;
            }

            if (logEvents)
                Debug.Log($"[Hatchery] Spend {hatchGemCost} gems to start hatching.");

            if (FloatingTextManager.Instance != null)
            {
                Vector3 pos = transform.position + Vector3.up * spentTextOffsetY;
                FloatingTextManager.Instance.ShowText("-" + hatchGemCost.ToString(), pos, Color.cyan, 1.1f);
            }
        }

        StartHatching(egg);
    }

    void StartHatching(Egg egg)
    {
        if (logEvents) Debug.Log($"[Hatchery] Start hatching egg: {egg.name}");

        currentEgg = egg;
        pendingEggForCost = null;
        hasShownNotEnoughForCurrentEgg = false;

        Rigidbody rb = egg.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Collider[] cols = egg.GetComponentsInChildren<Collider>();
        for (int i = 0; i < cols.Length; i++)
            cols[i].enabled = false;

        Transform parent = eggSocket != null ? eggSocket : transform;
        egg.transform.SetParent(parent);

        // ⚠ 不再直接瞬移，改成平滑飞过去
        StartCoroutine(MoveEggToSocketAndHatch(egg));
    }
    IEnumerator MoveEggToSocketAndHatch(Egg egg)
    {
        if (egg == null) yield break;

        Vector3 startPos = egg.transform.position;
        Quaternion startRot = egg.transform.rotation;

        Vector3 endPos = startPos;
        Quaternion endRot = startRot;

        if (eggSocket != null)
        {
            endPos = eggSocket.position;
            endRot = eggSocket.rotation;
        }

        float duration = Mathf.Max(0.01f, eggMoveDuration);
        float t = 0f;

        // 抛物线飞向孵蛋点
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float a = Mathf.Clamp01(t);

            Vector3 pos = Vector3.Lerp(startPos, endPos, a);
            float height = Mathf.Sin(a * Mathf.PI) * eggMoveHeight;
            pos.y += height;

            egg.transform.position = pos;
            egg.transform.rotation = Quaternion.Slerp(startRot, endRot, a);

            yield return null;
        }

        egg.transform.position = endPos;
        egg.transform.rotation = endRot;

        // 宝石充能演出（不会影响逻辑，只是特效）
        if (gemChargePrefab != null)
            StartCoroutine(GemChargeRoutine(egg.transform));

        // 真正开始计时孵化（原来的 HatchRoutine）
        StartCoroutine(HatchRoutine());
    }

    IEnumerator GemChargeRoutine(Transform eggTransform)
    {
        if (eggTransform == null) yield break;
        if (gemChargePrefab == null) yield break;

        Vector3 center = eggTransform.position;

        // 根据花费的宝石数量，决定显示的宝石个数
        int count = minGemChargeCount;
        if (hatchGemCost > 0 && gemVisualPerCost > 0)
        {
            count = Mathf.Clamp(hatchGemCost / gemVisualPerCost, minGemChargeCount, maxGemChargeCount);
        }

        Transform[] gems = new Transform[count];
        Vector3[] ringPositions = new Vector3[count];

        // 计算每个宝石在圈上的目标位置
        for (int i = 0; i < count; i++)
        {
            float angle = Mathf.PI * 2f * i / count;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * gemRingRadius;
            Vector3 pos = center + offset;
            pos.y += gemRingHeight;

            ringPositions[i] = pos;
        }

        // 决定宝石起点（玩家身上），如果没设就用蛋的位置代替
        Vector3 sourcePos;
        if (gemChargeSource != null)
            sourcePos = gemChargeSource.position + Vector3.up * gemSourceOffsetY;
        else
            sourcePos = center + Vector3.up * gemSourceOffsetY;

        // 先在玩家附近生成宝石，然后飞向圈上目标位置
        for (int i = 0; i < count; i++)
        {
            // 起点稍微随机一点，不全挤到一个点
            Vector3 start = sourcePos + new Vector3(
                Random.Range(-0.3f, 0.3f),
                Random.Range(-0.2f, 0.2f),
                Random.Range(-0.3f, 0.3f)
            );

            GameObject gObj = Instantiate(gemChargePrefab, start, Quaternion.identity);
            gems[i] = gObj.transform;

            // 防止误拾取/物理干扰
            var pickup = gObj.GetComponent<GemPickup>();
            if (pickup != null) Destroy(pickup);
            var col = gObj.GetComponent<Collider>();
            if (col != null) col.enabled = false;
            var rb = gObj.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            // 开始从玩家飞向圈的动画
            StartCoroutine(GemAppearFromSourceToRingRoutine(gems[i], start, ringPositions[i]));
        }

        // 等待一段时间，让整圈“成型”
        yield return new WaitForSeconds(gemRingAppearDuration);

        // 然后一个接一个地从圈飞进蛋里（保持你原来的感觉）
        for (int i = 0; i < count; i++)
        {
            Transform gem = gems[i];
            if (gem != null)
            {
                StartCoroutine(GemFlyToEggRoutine(gem, gem.position, eggTransform.position));
            }

            yield return new WaitForSeconds(gemDelayBetween);
        }
    }

    IEnumerator GemAppearFromSourceToRingRoutine(Transform gem, Vector3 start, Vector3 ringPos)
    {
        if (gem == null) yield break;

        float duration = Mathf.Max(0.01f, gemRingAppearDuration);
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float a = Mathf.Clamp01(t);

            Vector3 pos = Vector3.Lerp(start, ringPos, a);
            float height = Mathf.Sin(a * Mathf.PI) * gemFlyHeight;   // 复用飞行高度，让它也是个小抛物线
            pos.y += height;

            if (gem != null)
                gem.position = pos;

            yield return null;
        }

        if (gem != null)
            gem.position = ringPos;
    }


    IEnumerator GemFlyToEggRoutine(Transform gem, Vector3 start, Vector3 target)
    {
        if (gem == null) yield break;

        float duration = Mathf.Max(0.01f, gemFlyDuration);
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float a = Mathf.Clamp01(t);

            Vector3 pos = Vector3.Lerp(start, target, a);
            float height = Mathf.Sin(a * Mathf.PI) * gemFlyHeight;
            pos.y += height;

            if (gem != null)
                gem.position = pos;

            yield return null;
        }

        if (gem != null)
            Destroy(gem.gameObject);
    }



    IEnumerator HatchRoutine()
    {
        isHatching = true;
        CurrentProgress = 0f;

        if (progressBarRoot != null)
            progressBarRoot.gameObject.SetActive(true);

        float t = 0f;
        while (t < hatchTime)
        {
            t += Time.deltaTime;
            CurrentProgress = Mathf.Clamp01(t / hatchTime);
            yield return null;
        }

        FinishHatch();
    }

    void FinishHatch()
    {
        if (logEvents) Debug.Log("[Hatchery] Hatch finished, spawn LittleDragon.");

        if (progressBarRoot != null)
            progressBarRoot.gameObject.SetActive(false);

        GameObject prefabToSpawn = littleDragonPrefab;

        if (currentEgg != null)
        {
            Egg eggComp = currentEgg.GetComponent<Egg>();
            if (eggComp != null && eggComp.overrideDragonPrefab != null)
                prefabToSpawn = eggComp.overrideDragonPrefab;
        }

        if (prefabToSpawn != null)
        {
            Transform spawn = dragonSpawnPoint != null ? dragonSpawnPoint : transform;
            GameObject dragon = Instantiate(prefabToSpawn, spawn.position, spawn.rotation);

            if (FloatingTextManager.Instance != null)
            {
                Vector3 pos = spawn.position + Vector3.up * hatchDoneTextOffsetY;
                FloatingTextManager.Instance.ShowText("Htch Done!", pos, Color.yellow, 1.3f);
            }
        }
        else
        {
            if (logEvents) Debug.LogWarning("[Hatchery] No dragon prefab to spawn!");
        }

        if (currentEgg != null)
            Destroy(currentEgg.gameObject);

        currentEgg = null;
        isHatching = false;
        CurrentProgress = 0f;

        if (progressFill != null)
            progressFill.fillAmount = 0f;
    }
}
