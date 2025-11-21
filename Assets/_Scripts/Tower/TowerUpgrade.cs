using UnityEngine;

public class TowerUpgrade : MonoBehaviour
{
    public GameObject nextLevelPrefab;
    public int upgradeCost = 50;
    public int sellRefund = 25;

    public bool HasNextLevel => nextLevelPrefab != null;
}
