using UnityEngine;

public class FloatingTextManager : MonoBehaviour
{
    public static FloatingTextManager Instance;

    public FloatingText floatingTextPrefab;
    public Transform worldRoot;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Debug.Log("[FloatingTextManager] Awake, instance set.");
    }

    public void ShowText(string content, Vector3 worldPosition, Color color, float size)
    {
        if (floatingTextPrefab == null)
        {
            Debug.LogWarning("[FloatingTextManager] floatingTextPrefab is not set.");
            return;
        }

        Transform parent = worldRoot != null ? worldRoot : null;

        Debug.Log($"[FloatingTextManager] ShowText: {content} at {worldPosition}");

        FloatingText ft = Instantiate(floatingTextPrefab, worldPosition, Quaternion.identity, parent);
        ft.Init(content, color, size);
    }
}
