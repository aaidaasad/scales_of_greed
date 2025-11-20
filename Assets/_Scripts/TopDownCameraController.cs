using UnityEngine;
using UnityEngine.InputSystem;

public class TopDownHybridCameraController : MonoBehaviour
{
    public Transform target;

    public float followLerpSpeed = 8f;

    public float minDistance = 12f;
    public float maxDistance = 40f;

    public float closePitch = 50f;
    public float farPitch = 72f;

    public float closeFOV = 60f;
    public float farFOV = 45f;

    public float zoomSensitivity = 0.02f;
    public float zoomSmoothTime = 0.2f;

    public float dragSpeed = 0.02f;

    public Vector2 limitMin = new Vector2(-30f, -30f);
    public Vector2 limitMax = new Vector2(30f, 30f);

    public float deadZoneWidth = 20f;
    public float deadZoneHeight = 12f;
    public float focusLerpSpeed = 4f;

    Camera cam;

    float distance;
    float targetDistance;
    float distanceVelocity;

    Vector2 lastMousePos;
    bool isDragging;

    Vector3 mapCenter;
    Vector3 focusPoint;
    bool focusInitialized = false;

    Vector3 dragOffset = Vector3.zero;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam != null) cam.orthographic = false;

        distance = Mathf.Clamp((minDistance + maxDistance) * 0.5f, minDistance, maxDistance);
        targetDistance = distance;
    }

    void LateUpdate()
    {
        if (cam == null) return;

        if (!focusInitialized)
        {
            float cx = (limitMin.x + limitMax.x) * 0.5f;
            float cz = (limitMin.y + limitMax.y) * 0.5f;
            mapCenter = new Vector3(cx, 0f, cz);
            focusPoint = mapCenter;
            focusInitialized = true;
        }

        HandleZoom();
        HandleDrag();
        UpdateFocusPoint();
        ApplyCameraTransform();
        ClampToBounds();
    }

    void HandleZoom()
    {
        if (Mouse.current != null)
        {
            float scroll = Mouse.current.scroll.ReadValue().y;

            if (Mathf.Abs(scroll) > 0.01f)
            {
                targetDistance -= scroll * zoomSensitivity;
            }
        }

        targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);

        distance = Mathf.SmoothDamp(distance, targetDistance, ref distanceVelocity, zoomSmoothTime);

        float t = Mathf.InverseLerp(minDistance, maxDistance, distance);
        float pitch = Mathf.Lerp(closePitch, farPitch, t);
        float fov = Mathf.Lerp(closeFOV, farFOV, t);

        cam.fieldOfView = fov;
        transform.rotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    void HandleDrag()
    {
        if (Mouse.current == null) return;

        var middle = Mouse.current.middleButton;
        if (middle.isPressed)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();

            if (!isDragging)
            {
                lastMousePos = mousePos;
                isDragging = true;
                return;
            }

            Vector2 delta = mousePos - lastMousePos;
            lastMousePos = mousePos;

            Vector3 forward = transform.forward;
            forward.y = 0f;
            forward.Normalize();

            Vector3 right = transform.right;
            right.y = 0f;
            right.Normalize();

            Vector3 move = (-right * delta.x + -forward * delta.y) * dragSpeed;
            dragOffset += move;
        }
        else
        {
            if (isDragging)
            {
                isDragging = false;
                dragOffset = Vector3.zero; // 松开中键立即回到基础视角
            }
        }
    }

    void UpdateFocusPoint()
    {
        if (target == null)
        {
            focusPoint = Vector3.Lerp(focusPoint, mapCenter, focusLerpSpeed * Time.deltaTime);
            return;
        }

        Vector3 playerXZ = new Vector3(target.position.x, 0f, target.position.z);
        Vector3 centerXZ = new Vector3(mapCenter.x, 0f, mapCenter.z);

        Vector3 offset = playerXZ - centerXZ;

        float halfW = deadZoneWidth * 0.5f;
        float halfH = deadZoneHeight * 0.5f;

        float dx = 0f;
        float dz = 0f;

        if (Mathf.Abs(offset.x) > halfW)
        {
            dx = Mathf.Sign(offset.x) * (Mathf.Abs(offset.x) - halfW);
        }

        if (Mathf.Abs(offset.z) > halfH)
        {
            dz = Mathf.Sign(offset.z) * (Mathf.Abs(offset.z) - halfH);
        }

        Vector3 desiredFocus = mapCenter + new Vector3(dx, 0f, dz);

        focusPoint = Vector3.Lerp(focusPoint, desiredFocus, focusLerpSpeed * Time.deltaTime);
    }

    void ApplyCameraTransform()
    {
        Vector3 basePos = focusPoint - transform.forward * distance;
        Vector3 desiredPos = basePos + dragOffset;

        transform.position = Vector3.Lerp(transform.position, desiredPos, followLerpSpeed * Time.deltaTime);
    }

    void ClampToBounds()
    {
        Vector3 p = transform.position;
        p.x = Mathf.Clamp(p.x, limitMin.x, limitMax.x);
        p.z = Mathf.Clamp(p.z, limitMin.y, limitMax.y);
        transform.position = p;
    }
}
