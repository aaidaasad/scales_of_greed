using UnityEngine;

public class SlowBullet : MonoBehaviour, ITowerProjectile
{
    public float speed = 10f;
    public float damage = 0f;
    public float lifeTime = 5f;
    public float slowFactor = 0.3f;
    public float slowDuration = 2f;
    public GameObject iceVFX;


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
        if (iceVFX != null)
        {
            Instantiate(iceVFX, target.position, Quaternion.identity);
        }

        EnemyHealth enemyHealth = target.GetComponent<EnemyHealth>();
        if (enemyHealth != null && damage > 0f)
        {
            enemyHealth.TakeDamage(damage);
        }

        EnemyMover mover = target.GetComponent<EnemyMover>();
        if (mover != null)
        {
            mover.ApplySlow(slowFactor, slowDuration);
        }

        Destroy(gameObject);
    }

}
