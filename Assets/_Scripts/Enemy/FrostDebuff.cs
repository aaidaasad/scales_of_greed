using UnityEngine;

public class FrostDebuff : MonoBehaviour
{
    EnemyMover mover;

    int stackCount = 0;
    float stackTimer = 0f;

    int stacksToFreeze = 2;
    float freezeDuration = 1f;
    float freezeTimer = 0f;

    float slowFactor = 0.7f;
    float slowDuration = 2f;

    bool isFrozen = false;
    float originalMoveSpeed = 0f;

    public GameObject freezeVFXPrefab;
    GameObject freezeVFXInstance;

    void Awake()
    {
        mover = GetComponent<EnemyMover>();
        if (mover != null)
        {
            originalMoveSpeed = mover.moveSpeed;
        }
    }

    void Update()
    {
        if (mover == null) return;

        if (isFrozen)
        {
            freezeTimer -= Time.deltaTime;

            if (freezeVFXInstance != null)
            {
                freezeVFXInstance.transform.position = transform.position;
            }

            if (freezeTimer <= 0f)
            {
                EndFreeze();
            }
            return;
        }

        if (stackCount > 0)
        {
            stackTimer -= Time.deltaTime;
            if (stackTimer <= 0f)
            {
                stackCount = 0;
                stackTimer = 0f;
            }
        }
    }

    public void ApplyStackingSlow(float factor, float duration, int stacksNeededToFreeze, float freezeDur)
    {
        if (mover == null) return;

        slowFactor = factor;
        slowDuration = duration;
        stacksToFreeze = stacksNeededToFreeze;
        freezeDuration = freezeDur;

        if (isFrozen)
        {
            freezeTimer = Mathf.Max(freezeTimer, freezeDuration);
            return;
        }

        stackCount++;
        stackTimer = slowDuration;

        if (stackCount >= stacksToFreeze)
        {
            StartFreeze();
        }
        else
        {
            mover.ApplySlow(slowFactor, slowDuration);
        }
    }

    void StartFreeze()
    {
        isFrozen = true;
        freezeTimer = freezeDuration;
        mover.moveSpeed = 0f;
        mover.ApplySlow(slowFactor, freezeDuration);

        if (freezeVFXPrefab != null)
        {
            freezeVFXInstance = Instantiate(freezeVFXPrefab, transform.position, Quaternion.identity);
            freezeVFXInstance.transform.SetParent(transform);
        }
    }

    void EndFreeze()
    {
        isFrozen = false;
        mover.moveSpeed = originalMoveSpeed;

        if (freezeVFXInstance != null)
        {
            Destroy(freezeVFXInstance);
        }

        stackCount = 0;
        stackTimer = 0f;
    }

    void OnDisable()
    {
        if (freezeVFXInstance != null)
        {
            Destroy(freezeVFXInstance);
        }

        if (mover != null)
        {
            mover.moveSpeed = originalMoveSpeed;
        }
    }
}
