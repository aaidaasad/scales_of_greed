using UnityEngine;

public class FireDragonBullet : MonoBehaviour, ITowerProjectile
{
    public float speed = 12f;
    public float damage = 8f;
    public float lifeTime = 5f;

    public float burnDuration = 3f;
    public float burnDps = 4f;

    public float splashRadius = 1.5f;
    public float splashDamageMultiplier = 0.5f;
    public float splashBurnMultiplier = 0.7f;
    public LayerMask enemyLayer;

    public GameObject hitVFX;
    public GameObject splashVFX;

    Transform target;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public void SetTarget(Transform t)
    {
        target = t;
    }

    void Update()
    {
        if (target == null)
        {
            transform.position += transform.forward * speed * Time.deltaTime;
            return;
        }

        Vector3 dir = target.position - transform.position;
        float distThisFrame = speed * Time.deltaTime;

        if (dir.magnitude <= distThisFrame)
        {
            HitTarget();
            return;
        }

        transform.position += dir.normalized * distThisFrame;
        transform.rotation = Quaternion.LookRotation(dir);
    }

    void HitTarget()
    {
        Vector3 hitPos = transform.position;

        if (hitVFX != null)
            Instantiate(hitVFX, hitPos, Quaternion.identity);

        if (splashRadius > 0.01f)
        {
            DoSplash(hitPos);
        }
        else
        {
            if (target != null)
            {
                ApplyFireEffect(target, damage, burnDuration, burnDps);
            }
        }

        Destroy(gameObject);
    }

    void DoSplash(Vector3 center)
    {
        if (splashVFX != null)
            Instantiate(splashVFX, center, Quaternion.identity);

        Collider[] hits = Physics.OverlapSphere(center, splashRadius, enemyLayer);
        if (hits == null || hits.Length == 0) return;

        foreach (var h in hits)
        {
            if (h == null) continue;
            Transform t = h.transform;
            float dmg = damage;
            float dur = burnDuration;
            float dps = burnDps;

            if (target == null || t != target)
            {
                dmg *= splashDamageMultiplier;
                dur *= splashBurnMultiplier;
                dps *= splashBurnMultiplier;
            }

            ApplyFireEffect(t, dmg, dur, dps);
        }
    }

    void ApplyFireEffect(Transform enemy, float dmg, float dur, float dps)
    {
        if (enemy == null) return;

        EnemyHealth health = enemy.GetComponent<EnemyHealth>();
        if (health != null && dmg > 0f)
        {
            health.TakeDamage(dmg);
        }

        EnemyBurn burn = enemy.GetComponent<EnemyBurn>();
        if (burn == null)
            burn = enemy.gameObject.AddComponent<EnemyBurn>();

        burn.ApplyBurn(dur, dps);
    }

    void OnDrawGizmosSelected()
    {
        if (splashRadius <= 0.01f) return;
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, splashRadius);
    }
}
