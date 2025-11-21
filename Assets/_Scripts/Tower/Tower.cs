using UnityEngine;

public class Tower : MonoBehaviour
{
    public float range = 8f;
    public float fireRate = 1f;
    public LayerMask enemyLayer;

    public GameObject bulletPrefab;
    public Transform firePoint;

    float fireCooldown = 0f;

    void Update()
    {
        fireCooldown -= Time.deltaTime;

        if (fireCooldown <= 0f)
        {
            Transform target = FindTarget();
            if (target != null)
            {
                RotateTowards(target);
                Shoot(target);
                fireCooldown = 1f / fireRate;
            }
        }
    }

    Transform FindTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, range, enemyLayer);
        if (hits.Length == 0) return null;

        float minDist = Mathf.Infinity;
        Transform closest = null;

        foreach (var hit in hits)
        {
            float d = Vector3.Distance(transform.position, hit.transform.position);
            if (d < minDist)
            {
                minDist = d;
                closest = hit.transform;
            }
        }

        return closest;
    }

    void RotateTowards(Transform target)
    {
        Vector3 dir = target.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion lookRot = Quaternion.LookRotation(dir);
        Vector3 euler = lookRot.eulerAngles;
        transform.rotation = Quaternion.Euler(0f, euler.y, 0f);
    }

    void Shoot(Transform target)
    {
        if (bulletPrefab == null || firePoint == null)
        {
            return;
        }

        GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        ITowerProjectile proj = bulletObj.GetComponent<ITowerProjectile>();
        if (proj != null)
        {
            proj.SetTarget(target);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
