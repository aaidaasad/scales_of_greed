using UnityEngine;

public class HoverInfo : MonoBehaviour
{
    [Header("显示用信息")]
    public string displayName;
    [TextArea]
    public string description;

    // 如果同一个物体上已经有 TowerInfo，可以自动从那边读
    void Reset()
    {
        TowerInfo info = GetComponent<TowerInfo>();
        if (info != null)
        {
            displayName = info.towerName;
            description = info.description;
        }
    }
}
