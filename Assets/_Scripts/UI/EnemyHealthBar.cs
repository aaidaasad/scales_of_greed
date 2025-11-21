using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    public EnemyHealth target;
    public Image fillImage;
    public Vector3 offset = new Vector3(0, 2f, 0);

    Camera cam;

    void Awake()
    {
        cam = Camera.main;
    }

    void Start()
    {
        if (target != null)
        {
            // 订阅事件
            target.OnHealthChanged += UpdateHealthBar;
            // 立刻初始化一次，避免一开始是空的
            UpdateHealthBar(targetMax(), targetMax());
        }
    }

    void OnDisable()
    {
        if (target != null)
        {
            target.OnHealthChanged -= UpdateHealthBar;
        }
    }

    float targetMax()
    {
        return target != null ? target.maxHealth : 1f;
    }

    void LateUpdate()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        if (cam == null)
            cam = Camera.main;

        // 跟随敌人头顶
        transform.position = target.transform.position + offset;

        // 永远朝向相机
        var forward = cam.transform.rotation * Vector3.forward;
        transform.rotation = Quaternion.LookRotation(forward);
    }

    private void UpdateHealthBar(float current, float max)
    {
        if (max <= 0f)
        {
            // max 异常时，直接当作没血了，避免除 0
            fillImage.fillAmount = 0f;
            return;
        }

        float f = Mathf.Clamp01(current / max);
        fillImage.fillAmount = f;
    }

}
