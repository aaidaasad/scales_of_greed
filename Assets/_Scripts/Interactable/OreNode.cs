using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public class OreNode : MonoBehaviour
{
    public int maxHits = 3;
    public GameObject gemPickupPrefab;
    public Transform gemSpawnPoint;

    public float hitDelay = 0.35f;

    public float shakeDuration = 0.15f;
    public float shakeStrength = 0.1f;

    public int flashCount = 3;
    public float flashInterval = 0.05f;

    public float gemDropRadius = 0.8f;
    public float gemPopDuration = 0.25f;
    public float gemPopHeight = 0.6f;
    public int minGemsPerHit = 1;
    public int maxGemsPerHit = 3;

    int hits;
    Vector3 originalLocalPosition;
    Renderer[] renderers;
    Color[][] originalColors;
    bool isAnimating;
    bool isDestroyed;

    void Awake()
    {
        originalLocalPosition = transform.localPosition;
        renderers = GetComponentsInChildren<Renderer>();

        if (renderers != null && renderers.Length > 0)
        {
            originalColors = new Color[renderers.Length][];

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null)
                {
                    originalColors[i] = new Color[0];
                    continue;
                }

                Material[] mats = renderers[i].materials;
                originalColors[i] = new Color[mats.Length];

                for (int j = 0; j < mats.Length; j++)
                {
                    Material m = mats[j];
                    if (m == null)
                    {
                        originalColors[i][j] = Color.white;
                        continue;
                    }

                    // 优先用 _Color，如果没有就用 _TintColor，再不行就给个默认值
                    if (m.HasProperty("_Color"))
                    {
                        originalColors[i][j] = m.GetColor("_Color");
                    }
                    else if (m.HasProperty("_TintColor"))
                    {
                        originalColors[i][j] = m.GetColor("_TintColor");
                    }
                    else
                    {
                        // 这个材质不支持颜色，随便存个白色占位
                        originalColors[i][j] = Color.white;
                    }
                }

            }
        }
        else
        {
            originalColors = new Color[0][];
        }
    }

    public void Mine()
    {
        if (!isActiveAndEnabled) return;
        if (!gameObject.activeInHierarchy) return;
        if (isDestroyed) return;

        StartCoroutine(HitRoutine());
    }

    IEnumerator HitRoutine()
    {
        // 为了和挖矿动画对齐：先等一小段时间再抖 + 掉宝石
        if (hitDelay > 0f)
        {
            yield return new WaitForSeconds(hitDelay);
        }

        hits++;

        StartCoroutine(Shake());
        StartCoroutine(FlashRed());
        SpawnGem();

        if (hits >= maxHits)
        {
            isDestroyed = true;
            // 等抖动播一点再销毁
            yield return new WaitForSeconds(shakeDuration * 0.8f);
            Destroy(gameObject);
        }
    }

    void SpawnGem()
    {
        if (gemPickupPrefab == null) return;

        int gemCount = Random.Range(minGemsPerHit, maxGemsPerHit + 1);

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

            // 位置从 start → end 的插值
            Vector3 pos = Vector3.Lerp(start, end, t);

            // 中间弹一下的高度（一个简单的小抛物线）
            float height = Mathf.Sin(t * Mathf.PI) * gemPopHeight;
            pos.y += height;

            gem.position = pos;
            yield return null;
        }

        if (gem != null)
        {
            gem.position = end;
        }
    }

    IEnumerator Shake()
    {
        if (isAnimating) yield break;
        isAnimating = true;

        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / shakeDuration;

            Vector3 randomOffset = Random.insideUnitSphere * shakeStrength * (1f - t);
            randomOffset.y = 0f;

            transform.localPosition = originalLocalPosition + randomOffset;
            yield return null;
        }

        transform.localPosition = originalLocalPosition;
        isAnimating = false;
    }

    IEnumerator FlashRed()
    {
        if (renderers == null || renderers.Length == 0 || originalColors == null)
            yield break;

        for (int i = 0; i < flashCount; i++)
        {
            SetRenderersRed();
            yield return new WaitForSeconds(flashInterval);
            RestoreRendererColors();
            yield return new WaitForSeconds(flashInterval);
        }

        RestoreRendererColors();
    }

    void SetRenderersRed()
    {
        if (renderers == null) return;

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer r = renderers[i];
            if (r == null) continue;

            Material[] mats = r.materials;
            for (int j = 0; j < mats.Length; j++)
            {
                Material m = mats[j];
                if (m == null) continue;

                // 检查并使用正确属性
                if (m.HasProperty("_Color"))
                {
                    m.SetColor("_Color", Color.red);
                }
                else if (m.HasProperty("_TintColor"))
                {
                    m.SetColor("_TintColor", Color.red);
                }
                // 否则跳过（粒子着色器不支持颜色）
            }
        }
    }

    void RestoreRendererColors()
    {
        if (renderers == null || originalColors == null) return;

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer r = renderers[i];
            if (r == null) continue;

            Material[] mats = r.materials;
            Color[] cols = originalColors[i];

            for (int j = 0; j < mats.Length && j < cols.Length; j++)
            {
                Material m = mats[j];
                if (m == null) continue;

                if (m.HasProperty("_Color"))
                {
                    m.SetColor("_Color", cols[j]);
                }
                else if (m.HasProperty("_TintColor"))
                {
                    m.SetColor("_TintColor", cols[j]);
                }
            }
        }
    }

}
