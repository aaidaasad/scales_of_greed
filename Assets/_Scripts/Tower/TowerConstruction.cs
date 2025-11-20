using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TowerConstruction : MonoBehaviour
{
    public float buildTime = 2f;
    public Image progressFill;
    public GameObject builtVisualRoot;
    public GameObject constructionVisualRoot;

    Tower tower;
    bool isBuilt;

    public bool IsBuilt => isBuilt;

    void Awake()
    {
        tower = GetComponent<Tower>();
        if (tower != null) tower.enabled = false;

        if (builtVisualRoot != null) builtVisualRoot.SetActive(false);
        if (constructionVisualRoot != null) constructionVisualRoot.SetActive(true);

        if (progressFill != null)
        {
            progressFill.fillAmount = 0f;
        }
    }

    void Start()
    {
        StartCoroutine(BuildRoutine());
    }

    IEnumerator BuildRoutine()
    {
        float t = 0f;

        while (t < buildTime)
        {
            t += Time.deltaTime;
            float f = buildTime > 0f ? t / buildTime : 1f;
            if (progressFill != null)
            {
                progressFill.fillAmount = Mathf.Clamp01(f);
            }
            yield return null;
        }

        if (tower != null) tower.enabled = true;

        if (builtVisualRoot != null) builtVisualRoot.SetActive(true);
        if (constructionVisualRoot != null) constructionVisualRoot.SetActive(false);

        if (progressFill != null)
        {
            progressFill.transform.parent.gameObject.SetActive(false);
        }


        isBuilt = true;
    }
}
