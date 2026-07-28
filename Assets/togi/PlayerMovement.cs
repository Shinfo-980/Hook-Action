using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerMovement : MonoBehaviour
{
    [Header("移動速度")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 9f;

    [Header("ジャンプ")]
    [SerializeField] private float jumpHeight = 1.5f;

    [Header("接地判定")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.25f;
    [SerializeField] private LayerMask groundLayer;

    [Header("重力")]
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float groundedVelocity = -2f;

    [Header("フック解除後の慣性")]
    [SerializeField] private float momentumDrag = 5f;
    [SerializeField] private float minimumMomentumSpeed = 0.1f;

    [Header("視点操作")]
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private float mouseSensitivity = 0.15f;
    [SerializeField] private float minLookAngle = -80f;
    [SerializeField] private float maxLookAngle = 80f;

    [Header("フック")]
    [SerializeField] private HookShotController hookShotController;

    private CharacterController characterController;
    private PlayerInput playerInput;

    private InputAction moveAction;
    private InputAction sprintAction;
    private InputAction lookAction;
    private InputAction jumpAction;

    private float verticalVelocity;
    private float cameraPitch;

    private bool isGrounded;

    // フック解除後に残る水平方向の速度
    private Vector3 momentumVelocity;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();

        moveAction = playerInput.actions["Move"];
        sprintAction = playerInput.actions["Sprint"];
        lookAction = playerInput.actions["Look"];
        jumpAction = playerInput.actions["Jump"];
    }

    private void OnEnable()
    {
        moveAction.Enable();
        sprintAction.Enable();
        lookAction.Enable();
        jumpAction.Enable();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        moveAction.Disable();
        sprintAction.Disable();
        lookAction.Disable();
        jumpAction.Disable();
    }

    private void Update()
    {
        CheckGround();
        Look();

        if (hookShotController != null &&
            hookShotController.IsPlayerPulling)
        {
            // フック側がCharacterControllerを動かしている間は
            // 通常移動と重力を停止する
            verticalVelocity = 0f;
            momentumVelocity = Vector3.zero;
            return;
        }

        Move();
        Jump();
        ApplyMomentum();
        ApplyGravity();
    }

    private void CheckGround()
    {
        if (groundCheck == null)
        {
            isGrounded = false;
            return;
        }

        isGrounded = Physics.CheckSphere(
            groundCheck.position,
            groundCheckRadius,
            groundLayer,
            QueryTriggerInteraction.Ignore
        );
    }

    private void Move()
    {
        Vector2 moveInput = moveAction.ReadValue<Vector2>();

        Vector3 moveDirection =
            transform.right * moveInput.x +
            transform.forward * moveInput.y;

        moveDirection = Vector3.ClampMagnitude(
            moveDirection,
            1f
        );

        float currentSpeed = sprintAction.IsPressed()
            ? sprintSpeed
            : walkSpeed;

        characterController.Move(
            moveDirection * currentSpeed * Time.deltaTime
        );
    }

    private void Look()
    {
        Vector2 lookInput = lookAction.ReadValue<Vector2>();

        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        cameraPitch -= mouseY;

        cameraPitch = Mathf.Clamp(
            cameraPitch,
            minLookAngle,
            maxLookAngle
        );

        cameraHolder.localRotation = Quaternion.Euler(
            cameraPitch,
            0f,
            0f
        );
    }

    private void Jump()
    {
        if (!isGrounded)
        {
            return;
        }

        if (!jumpAction.WasPressedThisFrame())
        {
            return;
        }

        verticalVelocity = Mathf.Sqrt(
            jumpHeight * -2f * gravity
        );
    }

    private void ApplyGravity()
    {
        if (isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = groundedVelocity;
        }

        verticalVelocity += gravity * Time.deltaTime;

        characterController.Move(
            Vector3.up *
            verticalVelocity *
            Time.deltaTime
        );
    }

    private void ApplyMomentum()
    {
        if (momentumVelocity.sqrMagnitude <=
            minimumMomentumSpeed * minimumMomentumSpeed)
        {
            momentumVelocity = Vector3.zero;
            return;
        }

        characterController.Move(
            momentumVelocity * Time.deltaTime
        );

        momentumVelocity = Vector3.MoveTowards(
            momentumVelocity,
            Vector3.zero,
            momentumDrag * Time.deltaTime
        );

        // 地面に着いたら慣性を止める
        if (isGrounded)
        {
            momentumVelocity = Vector3.zero;
        }
    }

    /// <summary>
    /// フック解除時の速度を慣性として受け取る
    /// </summary>
    public void SetHookMomentum(Vector3 releaseVelocity)
    {
        // 水平方向の慣性
        momentumVelocity = new Vector3(
            releaseVelocity.x,
            0f,
            releaseVelocity.z
        );

        // 上下方向は既存の重力処理に引き継ぐ
        verticalVelocity = releaseVelocity.y;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
        {
            return;
        }

        Gizmos.color = isGrounded
            ? Color.green
            : Color.red;

        Gizmos.DrawWireSphere(
            groundCheck.position,
            groundCheckRadius
        );
    }
}