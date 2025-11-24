using UnityEngine;
using UnityEngine.InputSystem;

public class HoverDetector : MonoBehaviour
{
    public Camera mainCamera;
    public LayerMask hoverLayerMask = ~0;
    public float maxDistance = 1000f;

    HoverInfo currentHover;

    void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    void Update()
    {
        if (mainCamera == null) return;
        if (Mouse.current == null) return;
        if (TooltipUI.Instance == null) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, hoverLayerMask))
        {
            HoverInfo info = hit.collider.GetComponentInParent<HoverInfo>();

            if (info != null)
            {
                if (info != currentHover)
                {
                    currentHover = info;
                    // Debug.Log("Hover on: " + info.displayName);
                }

                TooltipUI.Instance.Show(info.displayName, info.description, mousePos);
                return;
            }
        }

        currentHover = null;
        TooltipUI.Instance.Hide();
    }
}
