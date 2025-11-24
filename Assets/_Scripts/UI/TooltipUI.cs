using UnityEngine;
using TMPro;

public class TooltipUI : MonoBehaviour
{
    public static TooltipUI Instance { get; private set; }

    public TMP_Text nameText;
    public TMP_Text descriptionText;

    RectTransform rectTransform;
    Canvas canvas;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        Hide();
    }

    public void Show(string title, string desc, Vector2 screenPos)
    {
        if (nameText != null) nameText.text = title;
        if (descriptionText != null) descriptionText.text = desc;

        // UI 打开
        gameObject.SetActive(true);

        if (canvas == null) return;

        RectTransform canvasRect = canvas.transform as RectTransform;

        // 把屏幕坐标转成 Canvas 的局部坐标
        Vector2 anchoredPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out anchoredPos
        );

        anchoredPos += new Vector2(10f, -10f);
        rectTransform.anchoredPosition = anchoredPos;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
