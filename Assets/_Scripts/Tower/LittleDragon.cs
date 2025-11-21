using UnityEngine;

public class LittleDragon : MonoBehaviour
{
    public float moveSpeed = 6f;
    public float orbitRadius = 1.5f;
    public float orbitAngularSpeed = 120f;
    public float flyHeight = 3f;
    public Vector3 centerOffset;

    public float fireRate = 2f;
    public GameObject bulletPrefab;
    public Transform firePoint;

    public float retargetInterval = 0.2f;

    PlayerController player;
    Transform orbitTarget;
    EnemyHealth currentEnemy;

    float orbitAngleRad;
    float retargetTimer;
    float fireCooldown;
    bool angleInitialized;

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            orbitTarget = player.transform;
            SnapAngleToCurrentPosition(true);
        }
    }

    void Update()
    {
        if (player == null)
        {
            player = FindObjectOfType<PlayerController>();
        }

        UpdateTarget();
        UpdateMovement();
        UpdateFire();
    }

    void UpdateTarget()
    {
        if (currentEnemy != null &&
            currentEnemy.gameObject.activeInHierarchy &&
            currentEnemy.Current > 0f)
        {
            if (orbitTarget != currentEnemy.transform)
            {
                orbitTarget = currentEnemy.transform;
                SnapAngleToCurrentPosition(false);
            }
            return;
        }

        currentEnemy = null;

        retargetTimer -= Time.deltaTime;
        if (retargetTimer > 0f) return;
        retargetTimer = retargetInterval;

        EnemyHealth best = FindHighestHealthEnemy();

        if (best != null)
        {
            currentEnemy = best;
            Transform oldTarget = orbitTarget;
            orbitTarget = currentEnemy.transform;
            SnapAngleToCurrentPosition(oldTarget == null);
        }
        else
        {
            if (player != null)
            {
                Transform oldTarget = orbitTarget;
                if (orbitTarget != player.transform)
                {
                    orbitTarget = player.transform;
                    SnapAngleToCurrentPosition(oldTarget == null);
                }
            }
            else
            {
                orbitTarget = null;
            }
        }
    }

    EnemyHealth FindHighestHealthEnemy()
    {
        EnemyHealth[] enemies = FindObjectsOfType<EnemyHealth>();
        EnemyHealth best = null;
        float bestHealth = 0f;

        for (int i = 0; i < enemies.Length; i++)
        {
            var e = enemies[i];
            if (e == null) continue;
            if (!e.gameObject.activeInHierarchy) continue;
            if (e.Current <= 0f) continue;

            if (best == null || e.Current > bestHealth)
            {
                best = e;
                bestHealth = e.Current;
            }
        }

        return best;
    }

    void SnapAngleToCurrentPosition(bool forceSet)
    {
        if (orbitTarget == null) return;

        Vector3 center = GetOrbitCenter();
        Vector3 toDragon = transform.position - center;
        toDragon.y = 0f;

        if (toDragon.sqrMagnitude < 0.0001f)
        {
            toDragon = Vector3.forward;
        }

        float newAngle = Mathf.Atan2(toDragon.z, toDragon.x);

        if (forceSet || !angleInitialized)
        {
            orbitAngleRad = newAngle;
            angleInitialized = true;
        }
        else
        {
            orbitAngleRad = FixAngleContinuity(orbitAngleRad, newAngle);
        }
    }

    float FixAngleContinuity(float oldAngle, float newAngle)
    {
        float oldDeg = oldAngle * Mathf.Rad2Deg;
        float newDeg = newAngle * Mathf.Rad2Deg;
        float deltaDeg = Mathf.DeltaAngle(oldDeg, newDeg);
        float resultDeg = oldDeg + deltaDeg;
        return resultDeg * Mathf.Deg2Rad;
    }

    Vector3 GetOrbitCenter()
    {
        if (orbitTarget == null) return transform.position;
        return orbitTarget.position + centerOffset;
    }

    Vector3 GetOrbitPosition(float angleRad)
    {
        Vector3 center = GetOrbitCenter();
        Vector3 offsetXZ = new Vector3(Mathf.Cos(angleRad), 0f, Mathf.Sin(angleRad)) * orbitRadius;
        Vector3 pos = center + offsetXZ;
        pos.y = center.y + flyHeight;
        return pos;
    }

    void UpdateMovement()
    {
        if (orbitTarget == null) return;

        orbitAngleRad -= orbitAngularSpeed * Mathf.Deg2Rad * Time.deltaTime;

        Vector3 targetPos = GetOrbitPosition(orbitAngleRad);

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            moveSpeed * Time.deltaTime
        );

        Vector3 dir = targetPos - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                15f * Time.deltaTime
            );
        }
    }

    void UpdateFire()
    {
        if (currentEnemy == null) return;
        if (!currentEnemy.gameObject.activeInHierarchy) return;
        if (currentEnemy.Current <= 0f) return;
        if (bulletPrefab == null || firePoint == null) return;

        fireCooldown -= Time.deltaTime;
        if (fireCooldown > 0f) return;

        fireCooldown = 1f / fireRate;

        GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        ITowerProjectile proj = bulletObj.GetComponent<ITowerProjectile>();
        if (proj != null)
        {
            proj.SetTarget(currentEnemy.transform);
        }
    }
}
