using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;    // 子弹移动速度
    public float damage = 5f;    // 伤害
    public float lifeTime = 5f;  // 最长存活时间，防止飞太远不消失

    private Transform target;

    private void Start()
    {
        // 保险：几秒后自动销毁，避免内存堆积
        Destroy(gameObject, lifeTime);
    }

    public void SetTarget(Transform t)
    {
        target = t;
    }

    private void Update()
    {
        if (target == null)
        {
            // 目标死了或者丢失了，子弹直接消失
            Destroy(gameObject);
            return;
        }

        Vector3 dir = target.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        // 如果本帧的移动距离大于到目标的距离，就算“命中”
        if (dir.magnitude <= distanceThisFrame)
        {
            HitTarget();
            return;
        }

        // 朝目标移动
        transform.Translate(dir.normalized * distanceThisFrame, Space.World);

        // 让子弹朝向目标方向（可选）
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(dir);
    }

    private void HitTarget()
    {
        // 对目标造成伤害
        EnemyHealth enemyHealth = target.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage);
        }

        // 击中就销毁子弹
        Destroy(gameObject);
    }
}
