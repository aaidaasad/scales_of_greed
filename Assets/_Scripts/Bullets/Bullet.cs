using UnityEngine;

public class Bullet : MonoBehaviour, ITowerProjectile
{
    public float speed = 10f;
    public float damage = 5f;
    public float lifeTime = 5f;

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
        EnemyHealth enemyHealth = target.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}
