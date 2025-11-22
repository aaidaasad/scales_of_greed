using UnityEngine;

public class LootFlyout : MonoBehaviour
{
    public Vector3 targetPosition;
    public float duration = 0.6f;
    public float arcHeight = 2f;

    public bool enableGemPickupOnEnd = true;

    GemPickup gem;
    float timer;
    Vector3 startPos;

    void Awake()
    {
        gem = GetComponent<GemPickup>();
        if (gem != null)
        {
            // 先禁用 GemPickup，等飞完弧线再启用它的“漂浮+吸附”逻辑
            gem.enabled = false;
        }
    }

    void OnEnable()
    {
        startPos = transform.position;
        timer = 0f;
    }

    void Update()
    {
        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / duration);

        // 水平从起点到终点插值
        Vector3 pos = Vector3.Lerp(startPos, targetPosition, t);
        // 垂直方向用 sin 做一个小弧线
        pos.y += Mathf.Sin(t * Mathf.PI) * arcHeight;

        transform.position = pos;

        if (timer >= duration)
        {
            if (gem != null && enableGemPickupOnEnd)
            {
                gem.enabled = true; // 飞完了，恢复 GemPickup
            }

            enabled = false; // 自己脚本就可以关掉了
        }
    }
}
