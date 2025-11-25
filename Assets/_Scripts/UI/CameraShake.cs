using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    Vector3 originalLocalPos;
    float shakeTimer;
    float shakeMagnitude;

    void Awake()
    {
        Instance = this;
        originalLocalPos = transform.localPosition;
    }

    void Update()
    {
        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.deltaTime;
            Vector3 offset = Random.insideUnitSphere * shakeMagnitude;
            transform.localPosition = originalLocalPos + offset;

            if (shakeTimer <= 0f)
            {
                transform.localPosition = originalLocalPos;
            }
        }
    }

    public void Shake(float duration, float magnitude)
    {
        shakeTimer = duration;
        shakeMagnitude = magnitude;
    }
}
