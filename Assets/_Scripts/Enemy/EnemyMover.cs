using UnityEngine;

public class EnemyMover : MonoBehaviour
{
    public Transform[] waypoints;
    public float moveSpeed = 3f;
    public float reachThreshold = 0.1f;
    public float damageToBase = 1f;

    public Material slowOverlayMaterial;

    BaseHealth baseHealth;
    int currentIndex = 0;

    float slowMultiplier = 1f;
    float slowTimer = 0f;

    Renderer[] renderers;
    Material[][] originalMaterials;
    bool overlayApplied = false;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        if (renderers != null && renderers.Length > 0)
        {
            originalMaterials = new Material[renderers.Length][];
            for (int i = 0; i < renderers.Length; i++)
            {
                originalMaterials[i] = renderers[i].materials;
            }
        }
    }

    void Start()
    {
        baseHealth = FindObjectOfType<BaseHealth>();
    }

    void Update()
    {
        UpdateSlow();

        if (waypoints == null || waypoints.Length == 0)
            return;

        Transform target = waypoints[currentIndex];
        Vector3 dir = target.position - transform.position;
        Vector3 moveDir = dir.normalized;

        float currentSpeed = moveSpeed * slowMultiplier;
        transform.position += moveDir * currentSpeed * Time.deltaTime;

        if (moveDir.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(moveDir);
        }

        if (dir.magnitude <= reachThreshold)
        {
            currentIndex++;
            if (currentIndex >= waypoints.Length)
            {
                ReachDestination();
            }
        }
    }

    void UpdateSlow()
    {
        if (slowTimer > 0f)
        {
            slowTimer -= Time.deltaTime;

            if (!overlayApplied)
            {
                ApplyOverlay();
            }

            if (slowTimer <= 0f)
            {
                slowTimer = 0f;
                slowMultiplier = 1f;
                RemoveOverlay();
            }
        }
    }

    public void ApplySlow(float factor, float duration)
    {
        factor = Mathf.Clamp01(factor);
        if (factor <= 0f)
            factor = 0.01f;

        if (slowTimer <= 0f || factor < slowMultiplier)
        {
            slowMultiplier = factor;
            slowTimer = duration;
        }
        else if (Mathf.Approximately(factor, slowMultiplier) && duration > slowTimer)
        {
            slowTimer = duration;
        }
    }

    void ApplyOverlay()
    {
        if (slowOverlayMaterial == null || renderers == null || originalMaterials == null)
            return;

        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] baseMats = originalMaterials[i];
            if (baseMats == null) continue;

            Material[] newMats = new Material[baseMats.Length + 1];
            for (int j = 0; j < baseMats.Length; j++)
            {
                newMats[j] = baseMats[j];
            }
            newMats[newMats.Length - 1] = slowOverlayMaterial;
            renderers[i].materials = newMats;
        }

        overlayApplied = true;
    }

    void RemoveOverlay()
    {
        if (!overlayApplied || renderers == null || originalMaterials == null)
            return;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (originalMaterials[i] != null)
            {
                renderers[i].materials = originalMaterials[i];
            }
        }

        overlayApplied = false;
    }

    void ReachDestination()
    {
        if (baseHealth != null)
        {
            baseHealth.TakeDamage(damageToBase);
        }

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        Gizmos.color = Color.red;
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            if (waypoints[i] != null && waypoints[i + 1] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }
        }
    }
}
