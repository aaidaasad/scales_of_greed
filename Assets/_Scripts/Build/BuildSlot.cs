using UnityEngine;

public class BuildSlot : MonoBehaviour
{
    public GameObject[] towerPrefabs;
    public Transform buildPoint;

    bool hasBuilt;
    GameObject currentTower;

    public bool HasBuilt => hasBuilt;
    public GameObject CurrentTower => currentTower;

    public void BuildTower(int index)
    {
        if (hasBuilt) return;
        if (towerPrefabs == null || towerPrefabs.Length == 0) return;
        if (index < 0 || index >= towerPrefabs.Length) return;
        if (towerPrefabs[index] == null) return;

        Transform point = buildPoint != null ? buildPoint : transform;
        currentTower = Instantiate(towerPrefabs[index], point.position, point.rotation);
        hasBuilt = true;
    }

    public void UpgradeTower()
    {
        if (!hasBuilt) return;
        if (currentTower == null) return;

        TowerUpgrade upgrade = currentTower.GetComponent<TowerUpgrade>();
        if (upgrade == null) return;
        if (!upgrade.HasNextLevel) return;
        if (upgrade.nextLevelPrefab == null) return;

        int cost = upgrade.upgradeCost;
        if (GameManager.Instance != null)
        {
            if (!GameManager.Instance.SpendGems(cost))
            {
                return;
            }
        }

        Transform point = buildPoint != null ? buildPoint : transform;
        Quaternion rot = currentTower.transform.rotation;

        Destroy(currentTower);
        currentTower = Instantiate(upgrade.nextLevelPrefab, point.position, rot);
        hasBuilt = true;
    }

    public void SellTower()
    {
        if (!hasBuilt) return;

        if (currentTower != null)
        {
            TowerUpgrade upgrade = currentTower.GetComponent<TowerUpgrade>();
            if (upgrade != null && GameManager.Instance != null && upgrade.sellRefund > 0)
            {
                GameManager.Instance.AddGems(upgrade.sellRefund);
            }

            Destroy(currentTower);
        }

        currentTower = null;
        hasBuilt = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null && BuildUIManager.Instance != null)
        {
            BuildUIManager.Instance.Show(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null && BuildUIManager.Instance != null)
        {
            if (BuildUIManager.Instance.CurrentSlot == this)
            {
                BuildUIManager.Instance.Hide();
            }
        }
    }
}
