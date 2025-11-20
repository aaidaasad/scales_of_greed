using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    public EnemyHealth target;     // 血条跟随哪个敌人
    public Image fillImage;        // 红条
    public Vector3 offset = new Vector3(0, 2f, 0); // 血条高度偏移

    private Camera cam;

    private void Start()
    {
        cam = Camera.main;

        // 监听敌人血量变化
        if (target != null)
        {
            target.OnHealthChanged += UpdateHealthBar;
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // 血条跟随敌人头顶
        transform.position = target.transform.position + offset;

        // UI 永远面朝相机
        transform.LookAt(transform.position + cam.transform.forward);
    }

    private void UpdateHealthBar(float current, float max)
    {
        float f = current / max;
        fillImage.fillAmount = f;
    }
}
