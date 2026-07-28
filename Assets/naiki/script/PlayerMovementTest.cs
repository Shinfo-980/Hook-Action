using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementTest : MonoBehaviour
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

    [Header("視点操作")]
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private float mouseSensitivity = 0.15f;
    [SerializeField] private float minLookAngle = -80f;
    [SerializeField] private float maxLookAngle = 80f;

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
            verticalVelocity = 0f;
            return;
        }

        Move();
        Jump();
        ApplyGravity();
    }

    private void CheckGround()
    {
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

        moveDirection = Vector3.ClampMagnitude(moveDirection, 1f);

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
            Vector3.up * verticalVelocity * Time.deltaTime
        );
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
        {
            return;
        }

        Gizmos.DrawWireSphere(
            groundCheck.position,
            groundCheckRadius
        );
    }
    public void SpeedUp(float amount)//←ここから
    {
        walkSpeed += amount;
        sprintSpeed += amount;
        Debug.Log("移動速度アップ　現在速度:" + walkSpeed);
    }
    public void jumpUp(float amount)
    {
        jumpHeight += amount;
        Debug.Log("ジャンプ力アップ　現在のジャンプ力:" + jumpHeight);
    }
}