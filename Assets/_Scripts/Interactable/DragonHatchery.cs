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

        if (eggSocket != null)
        {
            egg.transform.position = eggSocket.position;
            egg.transform.rotation = eggSocket.rotation;
        }

        StartCoroutine(HatchRoutine());
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
