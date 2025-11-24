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

    TowerConstruction construction;

    void Awake()
    {
        construction = GetComponent<TowerConstruction>();
    }

    void Update()
    {
        // 🔒 升级中 / 建造中时禁止开火
        if (construction != null && !construction.IsBuilt)
            return;

        fireCooldown -= Time.deltaTime;

        if (fireCooldown <= 0f)
        {
            Transform[] targets = FindTargets(targetsPerShot, out int actualCount);
            if (actualCount > 0)
            {
                RotateTowards(targets[0]);
                Shoot(targets, actualCount);
                fireCooldown = 1f / fireRate;
            }
        }
    }

    // ================================
    // 🔍 多目标查找（你缺少的就是这个）
    // ================================
    Transform[] FindTargets(int maxTargets, out int actualCount)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, range, enemyLayer);

        int n = Mathf.Min(maxTargets, hits.Length);
        actualCount = n;
        if (n == 0) return null;

        Transform[] results = new Transform[n];

        float[] distances = new float[hits.Length];
        for (int i = 0; i < hits.Length; i++)
        {
            distances[i] = Vector3.Distance(transform.position, hits[i].transform.position);
        }

        // 选出最近的 N 个
        for (int k = 0; k < n; k++)
        {
            float bestDist = Mathf.Infinity;
            int bestIndex = -1;

            for (int i = 0; i < hits.Length; i++)
            {
                if (distances[i] < bestDist)
                {
                    bestDist = distances[i];
                    bestIndex = i;
                }
            }

            results[k] = hits[bestIndex].transform;
            distances[bestIndex] = Mathf.Infinity;
        }

        return results;
    }

    // ================================
    void RotateTowards(Transform target)
    {
        Vector3 dir = target.position - transform.position;
        dir.y = 0;
        if (dir != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(dir),
                Time.deltaTime * 10f
            );
        }
    }

    // 多颗子弹
    void Shoot(Transform[] targets, int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (targets[i] == null) continue;

            GameObject go = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

            ITowerProjectile proj = go.GetComponent<ITowerProjectile>();
            if (proj != null)
            {
                proj.SetTarget(targets[i]);
            }
        }
    }
}
