using UnityEngine;

public class MultiTargetTower : MonoBehaviour
{
    public float range = 8f;
    public float fireRate = 1f;
    public LayerMask enemyLayer;

    public GameObject bulletPrefab;
    public Transform firePoint;

    public int targetsPerShot = 2;

    float fireCooldown = 0f;

    void Update()
    {
        fireCooldown -= Time.deltaTime;

        if (fireCooldown <= 0f)
        {
            Transform[] targets = FindTargets(targetsPerShot, out int actualCount);
            if (actualCount > 0)
            {
                if (targets[0] != null)
                {
                    RotateTowards(targets[0]);
                }

                Shoot(targets, actualCount);
                fireCooldown = 1f / fireRate;
            }
        }
    }

    Transform[] FindTargets(int maxTargets, out int actualCount)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, range, enemyLayer);
        if (hits == null || hits.Length == 0)
        {
            actualCount = 0;
            return new Transform[0];
        }

        int len = hits.Length;
        float[] distances = new float[len];
        for (int i = 0; i < len; i++)
        {
            distances[i] = Vector3.Distance(transform.position, hits[i].transform.position);
        }

        int count = Mathf.Min(maxTargets, len);
        Transform[] results = new Transform[count];

        for (int k = 0; k < count; k++)
        {
            float minDist = Mathf.Infinity;
            int bestIndex = -1;

            for (int i = 0; i < len; i++)
            {
                if (distances[i] < minDist)
                {
                    minDist = distances[i];
                    bestIndex = i;
                }
            }

            if (bestIndex == -1)
            {
                count = k;
                break;
            }

            results[k] = hits[bestIndex].transform;
            distances[bestIndex] = Mathf.Infinity;
        }

        actualCount = count;
        return results;
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

    void Shoot(Transform[] targets, int count)
    {
        if (bulletPrefab == null || firePoint == null) return;

        for (int i = 0; i < count; i++)
        {
            Transform t = targets[i];
            if (t == null) continue;

            GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

            ITowerProjectile proj = bulletObj.GetComponent<ITowerProjectile>();
            if (proj != null)
            {
                proj.SetTarget(t);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
