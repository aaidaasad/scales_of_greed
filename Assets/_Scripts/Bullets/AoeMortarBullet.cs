using UnityEngine;

public class AoeMortarBullet : MonoBehaviour, ITowerProjectile
{
    public float damage = 20f;
    public float explosionRadius = 2.5f;
    public float flightTime = 1.2f;
    public float arcHeight = 4f;
    public float maxLifeTime = 5f;
    public GameObject explosionVFX;


    Vector3 startPos;
    Vector3 targetPos;
    Vector3 lastPos;
    float timer;
    bool hasTarget;

    void Start()
    {
        Destroy(gameObject, maxLifeTime);

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        lastPos = transform.position;
    }

    public void SetTarget(Transform t)
    {
        startPos = transform.position;

        if (t != null)
            targetPos = t.position;
        else
            targetPos = startPos + transform.forward * 5f;

        if (flightTime < 0.05f)
            flightTime = 0.05f;

        timer = 0f;
        hasTarget = true;
        lastPos = startPos;
    }

    void Update()
    {
        if (!hasTarget) return;

        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / flightTime);

        Vector3 pos = Vector3.Lerp(startPos, targetPos, t);
        float parabola = 4f * t * (1f - t);
        pos.y += parabola * arcHeight;

        transform.position = pos;

        Vector3 dir = pos - lastPos;
        if (dir.sqrMagnitude > 0.0001f)
        {
            transform.forward = dir.normalized;
        }
        lastPos = pos;

        if (t >= 1f)
        {
            Explode();
        }
    }

    void Explode()
    {
        if (explosionVFX != null)
        {
            Instantiate(explosionVFX, transform.position, Quaternion.identity);
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        for (int i = 0; i < hits.Length; i++)
        {
            EnemyHealth eh = hits[i].GetComponent<EnemyHealth>();
            if (eh != null)
            {
                eh.TakeDamage(damage);
            }
        }

        Destroy(gameObject);
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
