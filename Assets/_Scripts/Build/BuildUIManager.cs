using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildUIManager : MonoBehaviour
{
    public static BuildUIManager Instance { get; private set; }

    public GameObject panel;
    public Button[] towerButtons;
    public int[] towerCosts;

    public Button upgradeButton;
    public Button sellButton;

    public TMP_Text towerNameText;
    public TMP_Text descriptionText;
    public TMP_Text nextUpgradeText;
    public TMP_Text costText;
    public TMP_Text timeText;
    public Image iconImage;

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
        UpdateInfoPanel();
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
        if (currentSlot.HasBuilt) return;

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
        RefreshButtons();
        UpdateInfoPanel();
    }

    public void OnUpgradeButton()
    {
        if (currentSlot == null) return;

        currentSlot.UpgradeTower();
        RefreshButtons();
        UpdateInfoPanel();
    }

    public void OnSellButton()
    {
        if (currentSlot == null) return;

        currentSlot.SellTower();
        Hide();
    }

    void RefreshButtons()
    {
        if (panel == null || !panel.activeSelf) return;

        bool slotHasTower = currentSlot != null && currentSlot.HasBuilt;

        if (towerButtons != null)
        {
            for (int i = 0; i < towerButtons.Length; i++)
            {
                Button b = towerButtons[i];
                if (b == null) continue;

                if (slotHasTower)
                {
                    b.gameObject.SetActive(false);
                    continue;
                }

                bool hasTower = currentSlot != null
                                && !currentSlot.HasBuilt
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

        if (upgradeButton != null)
        {
            if (slotHasTower && currentSlot.CurrentTower != null)
            {
                TowerUpgrade upgrade = currentSlot.CurrentTower.GetComponent<TowerUpgrade>();
                if (upgrade != null && upgrade.HasNextLevel)
                {
                    bool canAfford = true;
                    if (GameManager.Instance != null)
                    {
                        canAfford = GameManager.Instance.HasEnoughGems(upgrade.upgradeCost);
                    }

                    upgradeButton.gameObject.SetActive(true);
                    upgradeButton.interactable = canAfford;
                }
                else
                {
                    upgradeButton.gameObject.SetActive(false);
                }
            }
            else
            {
                upgradeButton.gameObject.SetActive(false);
            }
        }

        if (sellButton != null)
        {
            if (slotHasTower)
            {
                sellButton.gameObject.SetActive(true);
                sellButton.interactable = true;
            }
            else
            {
                sellButton.gameObject.SetActive(false);
            }
        }
    }

    void UpdateInfoPanel()
    {
        if (towerNameText == null && descriptionText == null && nextUpgradeText == null && costText == null && timeText == null && iconImage == null)
            return;

        if (currentSlot == null)
        {
            if (towerNameText != null) towerNameText.text = "";
            if (descriptionText != null) descriptionText.text = "";
            if (nextUpgradeText != null) nextUpgradeText.text = "";
            if (costText != null) costText.text = "";
            if (timeText != null) timeText.text = "";
            if (iconImage != null) iconImage.gameObject.SetActive(false);
            return;
        }

        if (currentSlot.HasBuilt && currentSlot.CurrentTower != null)
        {
            GameObject towerObj = currentSlot.CurrentTower;
            TowerInfo info = towerObj.GetComponent<TowerInfo>();
            TowerUpgrade upgrade = towerObj.GetComponent<TowerUpgrade>();
            TowerConstruction construction = towerObj.GetComponent<TowerConstruction>();

            if (towerNameText != null)
            {
                if (info != null && !string.IsNullOrEmpty(info.towerName))
                    towerNameText.text = info.towerName;
                else
                    towerNameText.text = "Tower";
            }

            if (descriptionText != null)
            {
                if (info != null && !string.IsNullOrEmpty(info.description))
                    descriptionText.text = info.description;
                else
                    descriptionText.text = "";
            }

            if (iconImage != null)
            {
                if (info != null && info.icon != null)
                {
                    iconImage.gameObject.SetActive(true);
                    iconImage.sprite = info.icon;
                }
                else
                {
                    iconImage.gameObject.SetActive(false);
                }
            }

            if (upgrade != null && upgrade.HasNextLevel && upgrade.nextLevelPrefab != null)
            {
                TowerInfo nextInfo = upgrade.nextLevelPrefab.GetComponent<TowerInfo>();
                TowerConstruction nextConstruction = upgrade.nextLevelPrefab.GetComponent<TowerConstruction>();

                if (nextUpgradeText != null)
                {
                    if (nextInfo != null && !string.IsNullOrEmpty(nextInfo.nextLevelDescription))
                        nextUpgradeText.text = nextInfo.nextLevelDescription;
                    else if (nextInfo != null && !string.IsNullOrEmpty(nextInfo.description))
                        nextUpgradeText.text = nextInfo.description;
                    else
                        nextUpgradeText.text = "Next Lvel: Description";
                }

                if (costText != null)
                {
                    costText.text = "Upgrade Cost: " + upgrade.upgradeCost;
                }

                if (timeText != null)
                {
                    float t = 0f;
                    if (nextConstruction != null)
                        t = nextConstruction.buildTime;
                    else if (construction != null)
                        t = construction.buildTime;

                    if (t > 0f)
                        timeText.text = "Upgrade Time:" + t.ToString("0.0") + "s";
                    else
                        timeText.text = "";
                }
            }
            else
            {
                if (nextUpgradeText != null) nextUpgradeText.text = "Max level";
                if (costText != null) costText.text = "";
                if (timeText != null) timeText.text = "";
            }
        }
        else
        {
            if (towerNameText != null) towerNameText.text = "Empty Spot";
            if (descriptionText != null) descriptionText.text = "You can construct new buildings here.";
            if (nextUpgradeText != null) nextUpgradeText.text = "";
            if (costText != null) costText.text = "";
            if (timeText != null) timeText.text = "";
            if (iconImage != null) iconImage.gameObject.SetActive(false);
        }
    }

    void OnGemChanged(int value)
    {
        if (panel != null && panel.activeSelf)
        {
            RefreshButtons();
            UpdateInfoPanel();
        }
    }
}
