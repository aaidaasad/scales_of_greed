using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public class OreNode : MonoBehaviour
{
    public int maxHits = 3;
    public GameObject gemPickupPrefab;
    public Transform gemSpawnPoint;

    public float shakeDuration = 0.15f;
    public float shakeStrength = 0.1f;

    public int flashCount = 3;
    public float flashInterval = 0.05f;

    public float gemDropRadius = 1.5f;
    public float gemPopDuration = 0.2f;
    public float gemPopHeight = 0.6f;

    int hits;
    Vector3 originalLocalPosition;
    Renderer[] renderers;
    Color[][] originalColors;
    bool isAnimating;

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
                    originalColors[i][j] = mats[j].color;
                }
            }
        }
    }

    public void Mine()
    {
        if (isAnimating) return;

        hits++;
        DropGem();
        StartCoroutine(HitRoutine());
    }

    void DropGem()
    {
        if (gemPickupPrefab == null) return;

        Vector3 basePos = gemSpawnPoint != null
            ? gemSpawnPoint.position
            : transform.position + Vector3.up * 0.5f;

        Vector2 offset2D = Random.insideUnitCircle.normalized *
                           Random.Range(gemDropRadius * 0.5f, gemDropRadius);

        Vector3 targetPos = basePos + new Vector3(offset2D.x, 0f, offset2D.y);

        GameObject obj = Instantiate(gemPickupPrefab, basePos, Quaternion.identity);
        StartCoroutine(PopGem(obj.transform, basePos, targetPos));
    }

    IEnumerator PopGem(Transform gem, Vector3 start, Vector3 end)
    {
        if (gemPopDuration <= 0f)
        {
            if (gem != null) gem.position = end;
            yield break;
        }

        float t = 0f;

        while (t < 1f && gem != null)
        {
            t += Time.deltaTime / gemPopDuration;
            if (t > 1f) t = 1f;

            float height = Mathf.Sin(t * Mathf.PI) * gemPopHeight;
            Vector3 pos = Vector3.Lerp(start, end, t);
            pos.y += height;

            gem.position = pos;
            yield return null;
        }

        if (gem != null)
        {
            Vector3 pos = end;
            pos.y = end.y;
            gem.position = pos;
        }
    }

    IEnumerator HitRoutine()
    {
        isAnimating = true;

        float elapsed = 0f;
        Vector3 basePos = originalLocalPosition;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            Vector3 offset = Random.insideUnitSphere * shakeStrength;
            offset.y = 0f;
            transform.localPosition = basePos + offset;
            yield return null;
        }

        transform.localPosition = basePos;

        for (int i = 0; i < flashCount; i++)
        {
            SetRenderersRed();
            yield return new WaitForSeconds(flashInterval);
            RestoreRendererColors();
            yield return new WaitForSeconds(flashInterval);
        }

        isAnimating = false;

        if (hits >= maxHits)
        {
            Destroy(gameObject);
        }
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
                mats[j].color = Color.red;
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
            Color[] colors = originalColors[i];

            for (int j = 0; j < mats.Length && j < colors.Length; j++)
            {
                mats[j].color = colors[j];
            }
        }
    }
}
