using UnityEngine;
using System.Collections;

public class PotionVisuals : MonoBehaviour
{
    public Transform visualRoot;
    public float idleRotateSpeed = 90f;
    public float useTiltAngle = 40f;
    public float useShakeAmplitude = 3f;
    public float useShakeFrequency = 20f;

    Rigidbody rb;
    bool isUsing;
    Quaternion originalLocalRotation;
    Coroutine useRoutine;

    void Awake()
    {
        // ? ???????????????????????
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = GetComponentInParent<Rigidbody>();

        if (visualRoot == null) visualRoot = transform;
        originalLocalRotation = visualRoot.localRotation;
    }

    void Update()
    {
        // ? ???????????????? isKinematic ??
        bool onGroundOrIdle = true;
        if (rb != null)
        {
            onGroundOrIdle = !rb.isKinematic;
        }

        if (!isUsing && onGroundOrIdle)
        {
            visualRoot.Rotate(Vector3.up, idleRotateSpeed * Time.deltaTime, Space.World);
        }
    }

    public void StartUse()
    {
        if (isUsing) return;

        if (visualRoot == null) visualRoot = transform;
        originalLocalRotation = visualRoot.localRotation;

        isUsing = true;

        if (useRoutine != null)
            StopCoroutine(useRoutine);

        useRoutine = StartCoroutine(UseRoutine());
    }

    public void StopUse()
    {
        isUsing = false;

        if (useRoutine != null)
        {
            StopCoroutine(useRoutine);
            useRoutine = null;
        }

        if (visualRoot != null)
            visualRoot.localRotation = originalLocalRotation;
    }

    IEnumerator UseRoutine()
    {
        float t = 0f;

        while (isUsing)
        {
            t += Time.deltaTime;

            float shake = Mathf.Sin(t * useShakeFrequency) * useShakeAmplitude;
            float tilt = useTiltAngle + shake;

            Quaternion tiltRot = Quaternion.Euler(-tilt, 0f, 0f);
            visualRoot.localRotation = originalLocalRotation * tiltRot;

            yield return null;
        }

        if (visualRoot != null)
            visualRoot.localRotation = originalLocalRotation;
    }
}
