using UnityEngine;
using UnityEngine.UI;

public class BuildUIManager : MonoBehaviour
{
    public static BuildUIManager Instance { get; private set; }

    public GameObject panel;
    public Button[] towerButtons;
    public int[] towerCosts;

    BuildSlot currentSlot;

    public BuildSlot CurrentSlot => currentSlot;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGemChanged += OnGemChanged;
        }
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGemChanged -= OnGemChanged;
        }
    }

    public void Show(BuildSlot slot)
    {
        currentSlot = slot;

        if (panel != null)
        {
            panel.SetActive(true);
        }

        RefreshButtons();
    }

    public void Hide()
    {
        currentSlot = null;

        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    public void BuildTower(int index)
    {
        if (currentSlot == null) return;

        int cost = 0;
        if (towerCosts != null && index >= 0 && index < towerCosts.Length)
        {
            cost = towerCosts[index];
        }

        if (GameManager.Instance != null)
        {
            if (!GameManager.Instance.SpendGems(cost))
            {
                return;
            }
        }

        currentSlot.BuildTower(index);
        Hide();
    }

    void RefreshButtons()
    {
        if (towerButtons == null) return;

        for (int i = 0; i < towerButtons.Length; i++)
        {
            Button b = towerButtons[i];
            if (b == null) continue;

            bool hasTower = currentSlot != null
                            && currentSlot.HasBuilt == false
                            && currentSlot.towerPrefabs != null
                            && i >= 0
                            && i < currentSlot.towerPrefabs.Length
                            && currentSlot.towerPrefabs[i] != null;

            if (!hasTower)
            {
                b.gameObject.SetActive(false);
                continue;
            }

            b.gameObject.SetActive(true);

            int cost = 0;
            if (towerCosts != null && i >= 0 && i < towerCosts.Length)
            {
                cost = towerCosts[i];
            }

            bool canAfford = true;
            if (GameManager.Instance != null)
            {
                canAfford = GameManager.Instance.HasEnoughGems(cost);
            }

            b.interactable = canAfford;
        }
    }

    void OnGemChanged(int value)
    {
        if (panel != null && panel.activeSelf)
        {
            RefreshButtons();
        }
    }
}
