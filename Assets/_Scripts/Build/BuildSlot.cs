using UnityEngine;

public class BuildSlot : MonoBehaviour
{
    public GameObject[] towerPrefabs;
    public Transform buildPoint;

    bool hasBuilt;

    public bool HasBuilt => hasBuilt;

    public void BuildTower(int index)
    {
        if (hasBuilt) return;
        if (towerPrefabs == null || towerPrefabs.Length == 0) return;
        if (index < 0 || index >= towerPrefabs.Length) return;
        if (towerPrefabs[index] == null) return;

        Transform point = buildPoint != null ? buildPoint : transform;
        Instantiate(towerPrefabs[index], point.position, point.rotation);
        hasBuilt = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasBuilt) return;

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
