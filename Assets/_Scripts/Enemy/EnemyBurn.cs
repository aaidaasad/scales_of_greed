using UnityEngine;
using System.Collections;

public class EnemyBurn : MonoBehaviour
{
    public float tickInterval = 0.5f;
    public GameObject burnVFX;

    EnemyHealth health;
    Coroutine burnRoutine;
    GameObject burnVfxInstance;

    void Awake()
    {
        health = GetComponent<EnemyHealth>();
    }

    public void ApplyBurn(float duration, float dps)
    {
        if (health == null) return;
        if (duration <= 0f || dps <= 0f) return;

        if (burnRoutine != null)
        {
            StopCoroutine(burnRoutine);
        }

        burnRoutine = StartCoroutine(BurnRoutine(duration, dps));
    }

    IEnumerator BurnRoutine(float duration, float dps)
    {
        if (burnVFX != null && burnVfxInstance == null)
        {
            burnVfxInstance = Instantiate(burnVFX, transform.position, Quaternion.identity, transform);
        }

        float time = 0f;
        float tickTimer = 0f;

        while (time < duration && health != null)
        {
            float dt = Time.deltaTime;
            time += dt;
            tickTimer += dt;

            while (tickTimer >= tickInterval)
            {
                tickTimer -= tickInterval;
                float damageThisTick = dps * tickInterval;
                health.TakeDamage(damageThisTick);
            }

            yield return null;
        }

        if (burnVfxInstance != null)
        {
            Destroy(burnVfxInstance);
            burnVfxInstance = null;
        }

        burnRoutine = null;
    }

    void OnDisable()
    {
        if (burnRoutine != null)
        {
            StopCoroutine(burnRoutine);
            burnRoutine = null;
        }

        if (burnVfxInstance != null)
        {
            Destroy(burnVfxInstance);
            burnVfxInstance = null;
        }
    }
}
