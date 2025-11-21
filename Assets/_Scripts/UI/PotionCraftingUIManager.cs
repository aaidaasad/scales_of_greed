using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PotionCraftingUIManager : MonoBehaviour
{
    public static PotionCraftingUIManager Instance { get; private set; }

    [Header("Panel")]
    public GameObject panel;

    [Header("Potion Buttons")]
    public Button[] potionButtons;
    public GameObject[] potionPrefabs;
    public int[] gemCosts;
    public float[] craftTimes;

    [Header("Texts (Optional)")]
    public TMP_Text[] nameTexts;
    public TMP_Text[] costTexts;
    public TMP_Text[] timeTexts;

    AlchemyStation currentStation;
    public AlchemyStation CurrentStation => currentStation;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (panel != null)
            panel.SetActive(false);

        if (potionButtons != null)
        {
            for (int i = 0; i < potionButtons.Length; i++)
            {
                int index = i;
                if (potionButtons[i] != null)
                    potionButtons[i].onClick.AddListener(() => OnClickPotion(index));
            }
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGemChanged += OnGemChanged;
        }

        RefreshTexts();
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGemChanged -= OnGemChanged;
        }
    }

    void RefreshTexts()
    {
        int count = potionButtons.Length;

        for (int i = 0; i < count; i++)
        {
            // ---- 名称 ----
            if (nameTexts != null && i < nameTexts.Length && nameTexts[i] != null)
            {
                if (potionPrefabs != null && i < potionPrefabs.Length && potionPrefabs[i] != null)
                {
                    nameTexts[i].text = potionPrefabs[i].name;
                }
                else
                {
                    nameTexts[i].text = "Potion";
                }
            }

            // ---- 花费 ----
            if (costTexts != null && i < costTexts.Length && costTexts[i] != null)
            {
                if (gemCosts != null && i < gemCosts.Length)
                    costTexts[i].text = "Cost: " + gemCosts[i];
                else
                    costTexts[i].text = "Cost: -";
            }

            // ---- 时间（加上 Time: XXXs）----
            if (timeTexts != null && i < timeTexts.Length && timeTexts[i] != null)
            {
                if (craftTimes != null && i < craftTimes.Length)
                    timeTexts[i].text = "Time: " + craftTimes[i].ToString("0.0") + "s";
                else
                    timeTexts[i].text = "Time: -";
            }
        }
    }


    void OnGemChanged(int value)
    {
        if (panel != null && panel.activeSelf)
        {
            RefreshButtons();
        }
    }

    public void OpenForStation(AlchemyStation station)
    {
        currentStation = station;

        if (panel != null)
            panel.SetActive(true);

        RefreshButtons();
    }

    public void Close()
    {
        currentStation = null;

        if (panel != null)
            panel.SetActive(false);
    }

    void RefreshButtons()
    {
        if (potionButtons == null) return;

        bool canUse = currentStation != null && !currentStation.IsCrafting;

        for (int i = 0; i < potionButtons.Length; i++)
        {
            bool interact = canUse;

            if (interact && GameManager.Instance != null && gemCosts != null && i < gemCosts.Length)
            {
                interact = GameManager.Instance.HasEnoughGems(gemCosts[i]);
            }

            if (potionButtons[i] != null)
                potionButtons[i].interactable = interact;
        }

        // 🔥 强制确保文本永远同步（避免按 Gem 更新时 UI 不刷）
        RefreshTexts();
    }

    void OnClickPotion(int index)
    {
        if (currentStation == null) return;
        if (potionPrefabs == null || index >= potionPrefabs.Length) return;
        if (gemCosts == null || index >= gemCosts.Length) return;
        if (craftTimes == null || index >= craftTimes.Length) return;

        if (GameManager.Instance != null)
        {
            if (!GameManager.Instance.SpendGems(gemCosts[index]))
                return;
        }

        currentStation.StartCraft(potionPrefabs[index], craftTimes[index], gemCosts[index]);
        Close();

        Close();
    }
}
