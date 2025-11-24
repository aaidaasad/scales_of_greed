using UnityEngine;

public class Bullet : MonoBehaviour, ITowerProjectile
{
    public float speed = 10f;
    public float damage = 5f;
    public float lifeTime = 5f;

    [Header("Impact VFX")]
    public GameObject impactVfxPrefab;
    public Vector3 impactOffset;

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
            Destroy(gameObject);
            return;
        }

        Vector3 dir = target.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        if (dir.magnitude <= distanceThisFrame)
        {
            HitTarget();
            return;
        }

        transform.Translate(dir.normalized * distanceThisFrame, Space.World);

        Vector3 flatDir = new Vector3(dir.x, 0f, dir.z);
        if (flatDir.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.LookRotation(flatDir);
        }
    }

    void HitTarget()
    {
        // ⭐ 伤害
        EnemyHealth enemyHealth = target.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage);
        }

        // ⭐ 播放命中特效
        if (impactVfxPrefab != null)
        {
            Vector3 pos = target.position + impactOffset;
            GameObject vfx = Instantiate(impactVfxPrefab, pos, Quaternion.identity);
            Destroy(vfx, 2f); // 让特效自动消失
        }

        // ⭐ 销毁子弹
        Destroy(gameObject);
    }
}
