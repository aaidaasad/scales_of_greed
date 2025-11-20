using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float gravity = -9.81f;

    public Camera mainCamera;
    public float forwardCheckDistance = 2f;
    public LayerMask forwardCheckLayerMask = ~0;

    CharacterController controller;
    Vector3 moveInput;
    Vector3 velocity;
    GameObject forwardTarget;

    public GameObject ForwardTarget => forwardTarget;

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    void Update()
    {
        Vector3 move = new Vector3(moveInput.x, 0f, moveInput.z);
        if (move.sqrMagnitude > 1f)
        {
            move = move.normalized;
        }

        controller.Move(move * moveSpeed * Time.deltaTime);

        if (controller.isGrounded && velocity.y < 0f)
        {
            velocity.y = -1f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        UpdateRotationToMouse();
        UpdateForwardCheck();
        HandleInteractionInput();
    }

    void UpdateRotationToMouse()
    {
        if (mainCamera == null) return;
        if (Mouse.current == null) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        float planeY = transform.position.y;
        if (Mathf.Abs(ray.direction.y) < 0.0001f) return;

        float t = (planeY - ray.origin.y) / ray.direction.y;
        if (t <= 0f) return;

        Vector3 hitPoint = ray.origin + ray.direction * t;
        Vector3 lookDir = hitPoint - transform.position;
        lookDir.y = 0f;

        if (lookDir.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.LookRotation(lookDir);
        }
    }

    void UpdateForwardCheck()
    {
        Vector3 origin;
        if (controller != null)
        {
            origin = controller.bounds.center;
        }
        else
        {
            origin = transform.position + Vector3.up;
        }

        Vector3 dir = transform.forward;

        RaycastHit hit;
        if (Physics.Raycast(origin, dir, out hit, forwardCheckDistance, forwardCheckLayerMask, QueryTriggerInteraction.Ignore))
        {
            forwardTarget = hit.collider.gameObject;
        }
        else
        {
            forwardTarget = null;
        }

        Debug.DrawRay(origin, dir * forwardCheckDistance, forwardTarget != null ? Color.green : Color.red);
    }

    void HandleInteractionInput()
    {
        if (Mouse.current == null) return;
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;
        if (forwardTarget == null) return;

        OreNode ore = forwardTarget.GetComponent<OreNode>();
        if (ore != null)
        {
            ore.Mine();
        }
    }

    void OnMove(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();
        moveInput = new Vector3(input.x, 0f, input.y);
    }
}
