using UnityEngine;

public class FrostBullet : MonoBehaviour, ITowerProjectile
{
    public float speed = 10f;
    public float damage = 0f;
    public float lifeTime = 5f;

    public float slowFactor = 0.7f;
    public float slowDuration = 2f;
    public int stacksToFreeze = 2;
    public float freezeDuration = 1f;

    public GameObject iceVFX;
    public GameObject freezeVFX; // ❄️ 冰封特效（外层整块冰）

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
            transform.rotation = Quaternion.LookRotation(flatDir);
    }

    void HitTarget()
    {
        if (iceVFX != null)
            Instantiate(iceVFX, target.position, Quaternion.identity);

        if (target.TryGetComponent(out EnemyHealth health) && damage > 0f)
            health.TakeDamage(damage);

        if (!target.TryGetComponent(out FrostDebuff debuff))
            debuff = target.gameObject.AddComponent<FrostDebuff>();

        if (debuff.freezeVFXPrefab == null)
            debuff.freezeVFXPrefab = freezeVFX;

        debuff.ApplyStackingSlow(slowFactor, slowDuration, stacksToFreeze, freezeDuration);

        Destroy(gameObject);
    }
}
