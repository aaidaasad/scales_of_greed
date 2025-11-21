using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TowerConstruction : MonoBehaviour
{
    public float buildTime = 2f;
    public Image progressFill;
    public Transform progressBarRoot;
    public GameObject builtModel;
    public GameObject constructionModel;

    Tower tower;
    Camera cam;
    bool isBuilt;

    [Header("Floating Text")]
    public string startText = "Start Building";
    public string finishText = "Finish Building";
    public Color startColor = Color.cyan;
    public Color finishColor = Color.green;
    public float textSize = 1.2f;
    public float textOffsetY = 2f;

    public bool IsBuilt => isBuilt;

    void Awake()
    {
        tower = GetComponent<Tower>();
        cam = Camera.main;

        if (tower != null) tower.enabled = false;

        if (builtModel != null) builtModel.SetActive(false);
        if (constructionModel != null) constructionModel.SetActive(true);

        if (progressFill != null) progressFill.fillAmount = 0f;

        if (progressBarRoot == null && progressFill != null)
            progressBarRoot = progressFill.transform.parent;
        if (progressBarRoot != null)
            progressBarRoot.gameObject.SetActive(true);
    }

    void Start()
    {
        // ⭐ 建造开始 — 弹字
        if (FloatingTextManager.Instance != null && !string.IsNullOrEmpty(startText))
        {
            Vector3 pos = transform.position + Vector3.up * textOffsetY;
            FloatingTextManager.Instance.ShowText(startText, pos, startColor, textSize);
        }

        StartCoroutine(BuildRoutine());
    }

    void LateUpdate()
    {
        if (progressBarRoot == null || cam == null || isBuilt) return;

        Vector3 dir = progressBarRoot.position - cam.transform.position;
        if (dir.sqrMagnitude > 0.0001f)
            progressBarRoot.rotation = Quaternion.LookRotation(dir);
    }

    IEnumerator BuildRoutine()
    {
        float t = 0f;

        while (t < buildTime)
        {
            t += Time.deltaTime;
            float f = buildTime > 0f ? t / buildTime : 1f;
            if (progressFill != null)
                progressFill.fillAmount = Mathf.Clamp01(f);
            yield return null;
        }

        // ⭐ 建造完成 — 弹字
        if (FloatingTextManager.Instance != null && !string.IsNullOrEmpty(finishText))
        {
            Vector3 pos = transform.position + Vector3.up * textOffsetY;
            FloatingTextManager.Instance.ShowText(finishText, pos, finishColor, textSize);
        }

        if (tower != null) tower.enabled = true;

        if (builtModel != null) builtModel.SetActive(true);
        if (constructionModel != null) constructionModel.SetActive(false);

        if (progressBarRoot != null)
            progressBarRoot.gameObject.SetActive(false);
        else if (progressFill != null)
            progressFill.transform.parent.gameObject.SetActive(false);

        isBuilt = true;
    }
}
