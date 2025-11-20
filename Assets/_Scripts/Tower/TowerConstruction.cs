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
