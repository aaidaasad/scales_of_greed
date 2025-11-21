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

    CharacterController controller;
    Vector3 moveInput;
    Vector3 velocity;
    GameObject forwardTarget;

    int forwardHash;
    int rightHash;
    int isMiningHash;
    int lowerBodySpeedHash;

    bool isMining;
    float miningAfterTimer;

    bool isCarrying;
    Carryable carriedItem;

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
    }

    void Update()
    {
        if (miningAfterTimer > 0f)
        {
            miningAfterTimer -= Time.deltaTime;
            if (miningAfterTimer < 0f) miningAfterTimer = 0f;
        }

        Vector3 move = new Vector3(moveInput.x, 0f, moveInput.z);
        if (move.sqrMagnitude > 1f) move.Normalize();

        float currentSpeed = moveSpeed;

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
    }

    void HandleInteractionInput()
    {
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
    }

    void DropCarriedItem()
    {
        if (carriedItem != null)
        {
            carriedItem.OnDrop();
        }

        carriedItem = null;
        isCarrying = false;
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

    void UpdateForwardCheck()
    {
        if (controller == null) return;

        Vector3 origin = controller.bounds.center;

        if (Physics.Raycast(origin, transform.forward, out RaycastHit hit, forwardCheckDistance, forwardCheckLayerMask))
            forwardTarget = hit.collider.gameObject;
        else
            forwardTarget = null;

        Debug.DrawRay(origin, transform.forward * forwardCheckDistance, forwardTarget ? Color.green : Color.red);
    }

    void OnMove(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();
        moveInput = new Vector3(input.x, 0f, input.y);
    }
}
