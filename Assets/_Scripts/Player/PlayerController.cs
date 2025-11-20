using UnityEngine;
using UnityEngine.InputSystem;  

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float gravity = -9.81f;

    private CharacterController controller;
    private Vector3 moveInput;  // 来自输入系统的移动向量
    private Vector3 velocity;   // 用来做重力

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        // 根据输入移动（世界坐标系的 XZ 平面）
        Vector3 move = new Vector3(moveInput.x, 0f, moveInput.z);

        // 防止斜向移动比直线快
        if (move.sqrMagnitude > 1f)
            move = move.normalized;

        controller.Move(move * moveSpeed * Time.deltaTime);

        // 简单重力
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // 让角色面向移动方向（可选）
        if (move.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(move);
        }
    }

    // ⚠️ 注意：函数名必须叫 OnMove，和 PlayerInput 的 "Move" 动作对得上
    // Behavior = Send Messages 时，Input System 会自动调用它
    private void OnMove(InputValue value)
    {
        // Move 是一个 Vector2，x=左右，y=上下
        Vector2 input = value.Get<Vector2>();
        // 转成 3D 的 XZ 平面
        moveInput = new Vector3(input.x, 0f, input.y);
    }
}
