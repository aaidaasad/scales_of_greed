using UnityEngine;
using System.Collections.Generic;

public class SummonedOrbiter : MonoBehaviour
{
    [Header("飞行绕圈")]
    public float moveSpeed = 12f;
    public float orbitRadius = 3f;
    public float orbitAngularSpeed = 90f;
    public float flyingHeightOffset = 3f;

    [Header("锁定")]
    public float retargetInterval = 0.2f;
    public Transform player;

    [Header("攻击")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 1f;

    [Header("动画")]
    public Animator animator;

    public string flyIdleStateName = "Fly Float";
    public string flyMoveStateName = "Fly Forward";
    public string flyAttackStateName = "Fly Flame Attack";
    public string takeoffStateName = "Take Off";
    public string landStateName = "Land";

    public string groundIdleStateName = "Idle01";
    public string groundWalkStateName = "Walk";
    public string groundRunStateName = "Run";
    public string sleepStateName = "Sleep";

    public float attackAnimDuration = 0.5f;
    public float takeoffDuration = 0.6f;
    public float landDuration = 0.6f;

    [Header("地面跟随")]
    public float groundWalkSpeedThreshold = 0.05f;
    public float groundRunSpeedThreshold = 2f;
    public float followDistance = 2f;
    public float groundQueueSpacing = 1.5f;
    public float groundHeightOffset = 0f;
    public float groundFollowSpeed = 6f;
    public float groundRotationLerpSpeed = 10f;

    [Header("飞行细节")]
    public float flyBobAmplitude = 0.3f;
    public float flyBobFrequency = 3f;
    public float bankAngle = 25f;

    [Header("无聊睡觉")]
    public float boredMinTime = 8f;
    public float boredMaxTime = 15f;

    static readonly List<SummonedOrbiter> activeOrbiters = new List<SummonedOrbiter>();

    EnemyHealth currentTarget;
    float retargetTimer;
    float fireCooldown;

    bool isFlyingMode;

    float attackTimer;
    float takeoffTimer;
    float landTimer;

    int flyIdleHash;
    int flyMoveHash;
    int flyAttackHash;
    int takeoffHash;
    int landHash;
    int groundIdleHash;
    int groundWalkHash;
    int groundRunHash;
    int sleepHash;
    int currentHash;

    Vector3 lastPosition;
    float currentSpeed;

    Vector3 lastPlayerPosition;
    Vector3 lastPlayerMoveDir;

    Vector3 flyVelocity;
    float bobOffset;
    float idleBoredTimer;

    void OnEnable()
    {
        activeOrbiters.Add(this);
    }

    void OnDisable()
    {
        activeOrbiters.Remove(this);
    }

    void Start()
    {
        if (player == null)
        {
            var pc = FindObjectOfType<PlayerController>();
            if (pc != null) player = pc.transform;
        }

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        flyIdleHash = Animator.StringToHash(flyIdleStateName);
        flyMoveHash = Animator.StringToHash(flyMoveStateName);
        flyAttackHash = Animator.StringToHash(flyAttackStateName);
        takeoffHash = Animator.StringToHash(takeoffStateName);
        landHash = Animator.StringToHash(landStateName);
        groundIdleHash = Animator.StringToHash(groundIdleStateName);
        groundWalkHash = Animator.StringToHash(groundWalkStateName);
        groundRunHash = Animator.StringToHash(groundRunStateName);
        sleepHash = Animator.StringToHash(sleepStateName);

        lastPosition = transform.position;

        if (player != null)
        {
            lastPlayerPosition = player.position;
            Vector3 f = player.forward;
            f.y = 0f;
            if (f.sqrMagnitude < 0.001f) f = Vector3.forward;
            lastPlayerMoveDir = f.normalized;
        }

        bobOffset = Random.Range(0f, 10f);
        ResetBoredTimer();
    }

    void Update()
    {
        float dt = Time.deltaTime;
        Vector3 prevPos = transform.position;

        UpdatePlayerMoveDirection();

        retargetTimer -= dt;
        fireCooldown -= dt;
        attackTimer -= dt;
        takeoffTimer -= dt;
        landTimer -= dt;

        if (currentTarget == null || currentTarget.gameObject == null)
        {
            if (retargetTimer <= 0f)
            {
                retargetTimer = retargetInterval;
                AcquireHighestHealthTarget();
            }
        }

        bool hasTarget = currentTarget != null && currentTarget.gameObject != null;
        bool desiredFlying = hasTarget;

        if (desiredFlying != isFlyingMode)
        {
            isFlyingMode = desiredFlying;
            if (isFlyingMode)
            {
                takeoffTimer = takeoffDuration;
                landTimer = 0f;
            }
            else
            {
                landTimer = landDuration;
                takeoffTimer = 0f;
                flyVelocity = Vector3.zero;
            }
        }

        if (isFlyingMode && hasTarget)
        {
            Transform centerTransform = currentTarget.transform;
            Vector3 center = centerTransform.position;

            Vector3 toCenter = center - transform.position;
            toCenter.y = 0f;

            Vector3 targetPos;

            if (toCenter.sqrMagnitude > orbitRadius * orbitRadius * 1.1f)
            {
                Vector3 dir = toCenter.normalized;
                targetPos = center - dir * orbitRadius;
            }
            else
            {
                int index;
                int count;
                GetOrbitGroupIndex(centerTransform, out index, out count);

                float baseAngleDeg = Time.time * orbitAngularSpeed;
                float finalAngleDeg = baseAngleDeg + (count > 1 ? 360f * index / count : 0f);
                float rad = finalAngleDeg * Mathf.Deg2Rad;

                Vector3 offset = new Vector3(Mathf.Cos(rad), 0f, -Mathf.Sin(rad));
                targetPos = center + offset * orbitRadius;
            }

            float bob = Mathf.Sin(Time.time * flyBobFrequency + bobOffset) * flyBobAmplitude;
            targetPos += Vector3.up * (flyingHeightOffset + bob);

            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref flyVelocity, 0.15f, moveSpeed);

            Vector3 lookDir = center - transform.position;
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude > 0.001f)
            {
                Vector3 horVel = new Vector3(flyVelocity.x, 0f, flyVelocity.z);
                float bank = 0f;
                if (horVel.sqrMagnitude > 0.001f)
                {
                    Vector3 right = Vector3.Cross(Vector3.up, lookDir.normalized);
                    float side = Vector3.Dot(horVel.normalized, right);
                    bank = Mathf.Clamp(side * bankAngle, -bankAngle, bankAngle);
                }

                Quaternion targetRot = Quaternion.LookRotation(lookDir);
                targetRot *= Quaternion.Euler(0f, 0f, bank);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, dt * 8f);
            }

            if (fireCooldown <= 0f && bulletPrefab != null)
            {
                Shoot(currentTarget.transform);
                fireCooldown = 1f / fireRate;
            }
        }
        else
        {
            if (player != null)
            {
                Vector3 dir = lastPlayerMoveDir;
                if (dir.sqrMagnitude < 0.001f)
                {
                    Vector3 toPlayer = player.position - transform.position;
                    toPlayer.y = 0f;
                    if (toPlayer.sqrMagnitude > 0.001f)
                        dir = toPlayer.normalized;
                    else
                        dir = Vector3.forward;
                }

                int qIndex, qCount;
                GetGroundQueueIndex(out qIndex, out qCount);

                float dist = followDistance + qIndex * groundQueueSpacing;

                Vector3 targetPos = player.position - dir * dist;
                targetPos.y = player.position.y + groundHeightOffset;

                transform.position = Vector3.MoveTowards(transform.position, targetPos, groundFollowSpeed * dt);
            }
        }

        Vector3 disp = transform.position - prevPos;
        Vector3 dispHor = new Vector3(disp.x, 0f, disp.z);
        currentSpeed = dispHor.magnitude / Mathf.Max(dt, 0.0001f);

        if (!isFlyingMode && dispHor.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dispHor.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, groundRotationLerpSpeed * dt);
        }

        UpdateAnimation();
        lastPosition = transform.position;
    }

    void UpdatePlayerMoveDirection()
    {
        if (player == null) return;

        Vector3 delta = player.position - lastPlayerPosition;
        Vector3 deltaXZ = new Vector3(delta.x, 0f, delta.z);

        if (deltaXZ.sqrMagnitude > 0.0001f)
        {
            lastPlayerMoveDir = deltaXZ.normalized;
        }

        lastPlayerPosition = player.position;
    }

    void GetOrbitGroupIndex(Transform center, out int index, out int count)
    {
        index = 0;
        count = 0;

        for (int i = 0; i < activeOrbiters.Count; i++)
        {
            var o = activeOrbiters[i];
            if (o == null) continue;
            if (o.currentTarget == null || o.currentTarget.gameObject == null) continue;

            if (o.currentTarget.transform == center)
            {
                if (o == this) index = count;
                count++;
            }
        }

        if (count == 0)
        {
            count = 1;
            index = 0;
        }
    }

    void GetGroundQueueIndex(out int index, out int count)
    {
        index = 0;
        count = 0;

        for (int i = 0; i < activeOrbiters.Count; i++)
        {
            var o = activeOrbiters[i];
            if (o == null) continue;
            if (o.player != player) continue;
            if (o.isFlyingMode) continue;

            if (o == this) index = count;
            count++;
        }

        if (count == 0)
        {
            count = 1;
            index = 0;
        }
    }

    void Shoot(Transform target)
    {
        Transform spawnPoint = firePoint != null ? firePoint : transform;
        GameObject bulletObj = Instantiate(bulletPrefab, spawnPoint.position, spawnPoint.rotation);

        ITowerProjectile proj = bulletObj.GetComponent<ITowerProjectile>();
        if (proj != null)
            proj.SetTarget(target);

        attackTimer = attackAnimDuration;
        ResetBoredTimer();
    }

    void AcquireHighestHealthTarget()
    {
        EnemyHealth[] all = FindObjectsOfType<EnemyHealth>();
        if (all == null || all.Length == 0)
        {
            currentTarget = null;
            return;
        }

        EnemyHealth best = null;
        float bestHealth = -1f;

        for (int i = 0; i < all.Length; i++)
        {
            EnemyHealth h = all[i];
            if (h == null || h.gameObject == null) continue;

            float hp = h.Current;
            if (hp > bestHealth)
            {
                bestHealth = hp;
                best = h;
            }
        }

        currentTarget = best;
        ResetBoredTimer();
    }

    void ResetBoredTimer()
    {
        idleBoredTimer = Random.Range(boredMinTime, boredMaxTime);
    }

    void UpdateAnimation()
    {
        if (animator == null) return;

        int targetHash;

        if (takeoffTimer > 0f)
        {
            targetHash = takeoffHash;
        }
        else if (landTimer > 0f)
        {
            targetHash = landHash;
        }
        else if (isFlyingMode)
        {
            if (attackTimer > 0f)
                targetHash = flyAttackHash;
            else if (currentSpeed > 0.05f)
                targetHash = flyMoveHash;
            else
                targetHash = flyIdleHash;
        }
        else
        {
            idleBoredTimer -= Time.deltaTime;

            if (idleBoredTimer <= 0f && currentSpeed < groundWalkSpeedThreshold)
            {
                targetHash = sleepHash;
                ResetBoredTimer();
            }
            else
            {
                if (currentSpeed < groundWalkSpeedThreshold)
                    targetHash = groundIdleHash;
                else if (currentSpeed < groundRunSpeedThreshold)
                    targetHash = groundWalkHash;
                else
                    targetHash = groundRunHash;
            }
        }

        if (targetHash != currentHash)
        {
            currentHash = targetHash;
            animator.CrossFade(currentHash, 0.15f);
        }
    }
}
