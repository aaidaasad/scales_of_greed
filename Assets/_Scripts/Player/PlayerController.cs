using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float gravity = -9.81f;

    public float miningActiveMoveMultiplier = 0.4f;
    public float miningAfterMoveMultiplier = 0.7f;
    public float miningAfterDuration = 0.3f;

    public float miningActiveAnimMultiplier = 0.5f;
    public float miningAfterAnimMultiplier = 0.8f;

    public float miningDuration = 0.7f;

    public float carryingMoveMultiplier = 0.5f;
    public float carryingAnimMultiplier = 0.8f;
    public Transform carryPoint;

    public Camera mainCamera;
    public float forwardCheckDistance = 2f;
    public LayerMask forwardCheckLayerMask = ~0;

    public Animator animator;
    public string forwardParam = "Forward";
    public string rightParam = "Right";
    public string isMiningParam = "IsMining";
    public string lowerBodySpeedParam = "LowerBodySpeed";

    public string drinkBoolParam = "IsDrinking";
    public string buffTriggerParam = "Buff";
    public string carryingUpperBoolParam = "IsCarryingUpper";

    public float buffAnimDuration = 0.8f;

    public float drinkMoveMultiplier = 0.2f;
    public float buffMoveMultiplier = 0.3f;

    public GameObject drinkVfxPrefab;
    public Transform drinkVfxPoint;
    public GameObject buffVfxPrefab;
    public Transform buffVfxPoint;

    public TrailRenderer potionTrail;

    public GameObject buffSmokePrefab;
    public Transform buffSmokePoint;
    public float buffSmokeSpawnInterval = 0.15f;
    public float buffSmokeLifetime = 0.8f;

    LightningPotion lightningBuff;
    float lightningBuffTime;


    CharacterController controller;
    Vector3 moveInput;
    Vector3 velocity;
    GameObject forwardTarget;

    int forwardHash;
    int rightHash;
    int isMiningHash;
    int lowerBodySpeedHash;
    int drinkBoolHash;
    int buffTriggerHash;
    int carryingUpperBoolHash;

    bool isMining;
    float miningAfterTimer;

    bool isCarrying;
    Carryable carriedItem;

    Potion carriedPotion;
    float potionDrinkTimer;

    bool hasPotionBuff;
    float potionBuffTimer;
    float potionBuffMultiplier = 1f;

    bool isDrinkingHold;
    bool isBuffAnimating;

    GameObject currentDrinkVfx;
    float buffSmokeTimer;

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        forwardHash = Animator.StringToHash(forwardParam);
        rightHash = Animator.StringToHash(rightParam);
        isMiningHash = Animator.StringToHash(isMiningParam);
        lowerBodySpeedHash = Animator.StringToHash(lowerBodySpeedParam);
        drinkBoolHash = Animator.StringToHash(drinkBoolParam);
        buffTriggerHash = Animator.StringToHash(buffTriggerParam);
        carryingUpperBoolHash = Animator.StringToHash(carryingUpperBoolParam);

        SetPotionTrail(false);
        buffSmokeTimer = 0f;
    }

    void Update()
    {
        if (miningAfterTimer > 0f)
        {
            miningAfterTimer -= Time.deltaTime;
            if (miningAfterTimer < 0f) miningAfterTimer = 0f;
        }

        if (potionBuffTimer > 0f)
        {
            potionBuffTimer -= Time.deltaTime;
            if (potionBuffTimer <= 0f && hasPotionBuff)
            {
                potionBuffTimer = 0f;
                hasPotionBuff = false;
                potionBuffMultiplier = 1f;
                SetPotionTrail(false);
            }
        }

        if (hasPotionBuff && buffSmokePrefab != null)
        {
            buffSmokeTimer -= Time.deltaTime;
            if (buffSmokeTimer <= 0f)
            {
                buffSmokeTimer = buffSmokeSpawnInterval;
                SpawnBuffSmokePuff();
            }
        }

        Vector3 move = new Vector3(moveInput.x, 0f, moveInput.z);
        if (move.sqrMagnitude > 1f) move.Normalize();

        float currentSpeed = moveSpeed;

        // ---- 新增：雷电曲线加速 ----
        if (hasPotionBuff && lightningBuff != null)
        {
            lightningBuffTime += Time.deltaTime;

            float t = lightningBuffTime / lightningBuff.speedCurveDuration;
            t = Mathf.Clamp01(t);

            float curveValue = lightningBuff.speedCurve.Evaluate(t);

            currentSpeed *= curveValue;

            // 曲线结束 → 自动恢复普通 buff
            if (t >= 1f)
            {
                lightningBuff = null; // 停用曲线
            }
        }


        if (isMining)
        {
            currentSpeed *= miningActiveMoveMultiplier;
        }
        else if (isCarrying)
        {
            currentSpeed *= carryingMoveMultiplier;
        }
        else if (miningAfterTimer > 0f)
        {
            currentSpeed *= miningAfterMoveMultiplier;
        }

        if (hasPotionBuff)
        {
            currentSpeed *= potionBuffMultiplier;
        }

        if (isDrinkingHold)
        {
            currentSpeed *= drinkMoveMultiplier;
        }
        else if (isBuffAnimating)
        {
            currentSpeed *= buffMoveMultiplier;
        }

        Vector3 worldMove = move * currentSpeed;
        controller.Move(worldMove * Time.deltaTime);

        if (controller.isGrounded && velocity.y < 0f)
            velocity.y = -1f;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        UpdateRotationToMouse();
        UpdateForwardCheck();
        UpdateAnimator(worldMove);
        HandleInteractionInput();
        HandlePotionDrinking();
    }

    void UpdateAnimator(Vector3 worldMove)
    {
        if (animator == null) return;

        Vector3 local = transform.InverseTransformDirection(worldMove);

        animator.SetFloat(forwardHash, local.z, 0.1f, Time.deltaTime);
        animator.SetFloat(rightHash, local.x, 0.1f, Time.deltaTime);

        float animSpeed = 1f;

        if (isMining)
        {
            animSpeed = miningActiveAnimMultiplier;
        }
        else if (isCarrying)
        {
            animSpeed = carryingAnimMultiplier;
        }
        else if (miningAfterTimer > 0f)
        {
            animSpeed = miningAfterAnimMultiplier;
        }

        animator.SetFloat(lowerBodySpeedHash, animSpeed);

        bool upperCarry = isCarrying && !isDrinkingHold && !isBuffAnimating;
        animator.SetBool(carryingUpperBoolHash, upperCarry);
    }

    void HandleInteractionInput()
    {
        if (isBuffAnimating) return;
        if (Mouse.current == null) return;

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            if (isCarrying)
                DropCarriedItem();
        }

        if (!Mouse.current.leftButton.wasPressedThisFrame) return;
        if (isMining) return;

        if (isCarrying) return;

        if (forwardTarget == null) return;

        Chest chest = forwardTarget.GetComponent<Chest>();
        if (chest != null)
        {
            chest.TryOpen();
            return;
        }


        Carryable carryable = forwardTarget.GetComponent<Carryable>();
        if (carryable != null)
        {
            PickupItem(carryable);
            return;
        }

        OreNode ore = forwardTarget.GetComponent<OreNode>();
        if (ore != null)
            StartCoroutine(MiningRoutine(ore));
    }

    void HandlePotionDrinking()
    {
        if (!isCarrying || carriedPotion == null)
        {
            StopDrinkState();
            return;
        }

        if (isBuffAnimating) return;
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.isPressed)
        {
            if (!isDrinkingHold)
            {
                StartDrinkState();
            }

            potionDrinkTimer += Time.deltaTime;

            if (potionDrinkTimer >= carriedPotion.drinkHoldTime && !isBuffAnimating)
            {
                StartCoroutine(DrinkCompleteRoutine());
            }
        }
        else
        {
            if (isDrinkingHold)
            {
                StopDrinkState();
            }
        }
    }

    void StartDrinkState()
    {
        isDrinkingHold = true;
        potionDrinkTimer = 0f;

        if (animator != null)
            animator.SetBool(drinkBoolHash, true);

        if (drinkVfxPrefab != null && currentDrinkVfx == null)
        {
            Transform p = drinkVfxPoint != null ? drinkVfxPoint : transform;
            currentDrinkVfx = Instantiate(drinkVfxPrefab, p.position, p.rotation, p);
        }

        if (carriedPotion != null)
        {
            PotionVisuals visuals = carriedPotion.GetComponent<PotionVisuals>();
            if (visuals != null)
                visuals.StartUse();
        }

        SetPotionTrail(true);
    }


    void StopDrinkState()
    {
        isDrinkingHold = false;
        potionDrinkTimer = 0f;

        if (animator != null)
            animator.SetBool(drinkBoolHash, false);

        if (currentDrinkVfx != null)
        {
            Destroy(currentDrinkVfx);
            currentDrinkVfx = null;
        }

        if (carriedPotion != null)
        {
            PotionVisuals visuals = carriedPotion.GetComponent<PotionVisuals>();
            if (visuals != null)
                visuals.StopUse();
        }

        if (!hasPotionBuff)
        {
            SetPotionTrail(false);
        }
    }


    IEnumerator DrinkCompleteRoutine()
    {
        isBuffAnimating = true;

        isDrinkingHold = false;
        potionDrinkTimer = 0f;

        if (animator != null)
            animator.SetBool(drinkBoolHash, false);

        if (currentDrinkVfx != null)
        {
            Destroy(currentDrinkVfx);
            currentDrinkVfx = null;
        }

        if (carriedPotion != null)
        {
            PotionVisuals visuals = carriedPotion.GetComponent<PotionVisuals>();
            if (visuals != null)
                visuals.StopUse();
        }

        if (animator != null)
            animator.SetTrigger(buffTriggerHash);

        if (buffVfxPrefab != null)
        {
            Transform p = buffVfxPoint != null ? buffVfxPoint : transform;
            GameObject vfx = Instantiate(buffVfxPrefab, p.position, p.rotation, p);
            Destroy(vfx, buffAnimDuration + 0.5f);
        }

        float t = 0f;
        while (t < buffAnimDuration)
        {
            t += Time.deltaTime;
            yield return null;
        }

        ApplyPotionBuff();

        if (carriedItem != null)
            Destroy(carriedItem.gameObject);

        carriedItem = null;
        carriedPotion = null;
        isCarrying = false;
        isBuffAnimating = false;
    }

    void ApplyPotionBuff()
    {
        if (carriedPotion == null) return;

        hasPotionBuff = true;
        potionBuffTimer = carriedPotion.buffDuration;
        potionBuffMultiplier = carriedPotion.moveSpeedMultiplier;

        lightningBuff = carriedPotion as LightningPotion;
        lightningBuffTime = 0f;

        if (FloatingTextManager.Instance != null)
        {
            Vector3 pos = transform.position + Vector3.up * 2f;
            FloatingTextManager.Instance.ShowText("Buff!", pos, Color.green, 1.2f);
        }

        if (FloatingTextManager.Instance != null)
        {
            Vector3 pos = transform.position + Vector3.up * 2f;
            FloatingTextManager.Instance.ShowText("Potion!", pos, Color.blue, 1.2f);
        }

        SetPotionTrail(true);
        buffSmokeTimer = 0f;

     
        LightningPotion lightning = carriedPotion as LightningPotion;
        if (lightning != null)
        {
            lightning.TriggerLightning();
        }
    }


    void SpawnBuffSmokePuff()
    {
        if (buffSmokePrefab == null) return;

        Transform p = buffSmokePoint != null ? buffSmokePoint : transform;
        Vector3 pos = p.position;
        GameObject puff = Instantiate(buffSmokePrefab, pos, Quaternion.identity);

        if (buffSmokeLifetime > 0f)
            Destroy(puff, buffSmokeLifetime);
    }

    void SetPotionTrail(bool active)
    {
        if (potionTrail == null) return;
        potionTrail.emitting = active;
    }

    IEnumerator MiningRoutine(OreNode ore)
    {
        isMining = true;
        if (animator != null)
            animator.SetBool(isMiningHash, true);

        if (ore != null)
            ore.Mine();

        float t = 0f;
        while (t < miningDuration)
        {
            t += Time.deltaTime;
            yield return null;
        }

        if (animator != null)
            animator.SetBool(isMiningHash, false);
        isMining = false;
        miningAfterTimer = miningAfterDuration;
    }

    void PickupItem(Carryable item)
    {
        if (item == null) return;
        if (carryPoint == null) return;

        carriedItem = item;
        isCarrying = true;
        item.OnPickup(carryPoint);

        carriedPotion = item.GetComponent<Potion>();
        potionDrinkTimer = 0f;
    }

    void DropCarriedItem()
    {
        if (carriedItem != null)
        {
            carriedItem.OnDrop();
        }

        carriedItem = null;
        carriedPotion = null;
        isCarrying = false;
        potionDrinkTimer = 0f;

        StopDrinkState();
    }

    void UpdateRotationToMouse()
    {
        if (mainCamera == null) return;
        if (Mouse.current == null) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        float planeY = transform.position.y;
        float t = (planeY - ray.origin.y) / ray.direction.y;

        if (t > 0f)
        {
            Vector3 hitPoint = ray.origin + ray.direction * t;
            Vector3 look = hitPoint - transform.position;
            look.y = 0f;

            if (look.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(look);
        }
    }

    Highlightable lastHighlight;

    void UpdateForwardCheck()
    {
        if (controller == null) return;

        Vector3 origin = controller.bounds.center;

        GameObject newTarget = null;

        if (Physics.Raycast(origin, transform.forward, out RaycastHit hit, forwardCheckDistance, forwardCheckLayerMask))
            newTarget = hit.collider.gameObject;

        // ====== 👇 这里是新增的高光逻辑，不会影响你的 forwardTarget ======
        if (lastHighlight != null && (newTarget == null || lastHighlight.gameObject != newTarget))
        {
            lastHighlight.SetHighlight(false);
            lastHighlight = null;
        }

        if (newTarget != null)
        {
            Highlightable h = newTarget.GetComponent<Highlightable>();
            if (h != null)
            {
                lastHighlight = h;
                lastHighlight.SetHighlight(true);
            }
        }
        // ==========================================================

        // 原功能保持不变
        forwardTarget = newTarget;

        Debug.DrawRay(origin, transform.forward * forwardCheckDistance,
            forwardTarget ? Color.green : Color.red);
    }


    void OnMove(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();
        moveInput = new Vector3(input.x, 0f, input.y);
    }



}

