using UnityEngine;

public class Tower : MonoBehaviour
{
    public float range = 8f;           // 攻击范围
    public float fireRate = 1f;        // 每秒几发子弹
    public LayerMask enemyLayer;       // 敌人所在 Layer

    public GameObject bulletPrefab;    // 子弹预制体
    public Transform firePoint;        // 子弹发射位置

    private float fireCooldown = 0f;

    private void Update()
    {
        fireCooldown -= Time.deltaTime;

        if (fireCooldown <= 0f)
        {
            // 找一个目标
            Transform target = FindTarget();
            if (target != null)
            {
                RotateTowards(target);
                Shoot(target);
                fireCooldown = 1f / fireRate;
            }
        }
    }

    // 在范围内找最近的敌人
    private Transform FindTarget()
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

    // 只在水平面旋转塔身体
    private void RotateTowards(Transform target)
    {
        Vector3 dir = target.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion lookRot = Quaternion.LookRotation(dir);
        Vector3 euler = lookRot.eulerAngles;
        transform.rotation = Quaternion.Euler(0f, euler.y, 0f);
    }

    private void Shoot(Transform target)
    {
        if (bulletPrefab == null || firePoint == null)
        {
            Debug.LogWarning("Tower: bulletPrefab 或 firePoint 没设置！");
            return;
        }

        GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.SetTarget(target);
        }
    }

    // Scene 视图中调试：画出攻击范围
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
