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

    [Header("フック中の移動操作")]
    [SerializeField] private float hookMoveControl = 0.35f;

    [Header("視点操作")]
    [SerializeField] private Transform cameraHolder;

    [Header("マウス感度")]
    [SerializeField] private float mouseSensitivity = 0.15f;

    [Header("コントローラー感度")]
    [SerializeField] private float controllerSensitivity = 180f;

    [Header("上下の視点制限")]
    [SerializeField] private float minLookAngle = -80f;
    [SerializeField] private float maxLookAngle = 80f;

    [Header("コントローラー設定")]
    [SerializeField] private float controllerDeadZone = 0.1f;
    [SerializeField] private bool invertControllerY = false;

    [Header("フック")]
    [SerializeField] private HookShotController hookShotController;

    private CharacterController characterController;
    private PlayerInput playerInput;

    private InputAction moveAction;
    private InputAction sprintAction;
    private InputAction lookAction;
    private InputAction controllerLookAction;
    private InputAction jumpAction;

    private float verticalVelocity;
    private float cameraPitch;

    private bool isGrounded;

    // フック解除後に残る慣性速度
    private Vector3 momentumVelocity;

    // アイテムによる永続速度強化
    private float speedBonus = 0f;

    // アイテムによる永続ジャンプ強化
    private float jumpBonus = 0f;

    // イベントによる一時的な倍率
    private float movementBuffMultiplier = 1f;
    private float jumpBuffMultiplier = 1f;

    private bool isSprintActive;

    private void Awake()
    {
        characterController =
            GetComponent<CharacterController>();

        playerInput =
            GetComponent<PlayerInput>();

        moveAction =
            playerInput.actions["Move"];

        sprintAction =
            playerInput.actions["Sprint"];

        lookAction =
            playerInput.actions["Look"];

        controllerLookAction =
            playerInput.actions["ControllerLook"];

        jumpAction =
            playerInput.actions["Jump"];
    }

    private void OnEnable()
    {
        moveAction.Enable();
        sprintAction.Enable();
        lookAction.Enable();
        controllerLookAction.Enable();
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
        controllerLookAction.Disable();
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
        Vector2 moveInput =
            moveAction.ReadValue<Vector2>();

        // Sprintボタンを押した瞬間にSprint状態へ
        if (sprintAction.WasPressedThisFrame())
        {
            isSprintActive = true;
        }

        // 移動入力が完全になくなったらSprint解除
        if (moveInput.sqrMagnitude <= 0.001f)
        {
            isSprintActive = false;
        }

        Vector3 moveDirection =
            transform.right * moveInput.x +
            transform.forward * moveInput.y;

        if (moveDirection.sqrMagnitude > 1f)
        {
            moveDirection.Normalize();
        }

        float baseMoveSpeed =
            isSprintActive
                ? sprintSpeed
                : walkSpeed;

        float currentSpeed =
            (baseMoveSpeed + speedBonus) *
            movementBuffMultiplier;

        characterController.Move(
            moveDirection *
            currentSpeed *
            Time.deltaTime
        );
    }

    private void Look()
    {
        Vector2 mouseInput =
            lookAction.ReadValue<Vector2>();

        Vector2 controllerInput =
            controllerLookAction.ReadValue<Vector2>();

        float horizontalLook = 0f;
        float verticalLook = 0f;

        /*
         * マウスは1フレーム内で動いた量が入力されるため、
         * Time.deltaTimeを掛けない。
         */
        if (mouseInput.sqrMagnitude > 0.0001f)
        {
            horizontalLook =
                mouseInput.x *
                mouseSensitivity;

            verticalLook =
                mouseInput.y *
                mouseSensitivity;
        }

        /*
         * スティックは倒している量が-1～1で入力され続けるため、
         * Time.deltaTimeを掛ける。
         */
        if (controllerInput.sqrMagnitude >
            controllerDeadZone *
            controllerDeadZone)
        {
            horizontalLook +=
                controllerInput.x *
                controllerSensitivity *
                Time.deltaTime;

            float controllerY =
                controllerInput.y;

            if (invertControllerY)
            {
                controllerY *= -1f;
            }

            verticalLook +=
                controllerY *
                controllerSensitivity *
                Time.deltaTime;
        }

        transform.Rotate(
            Vector3.up *
            horizontalLook
        );

        cameraPitch -= verticalLook;

        cameraPitch = Mathf.Clamp(
            cameraPitch,
            minLookAngle,
            maxLookAngle
        );

        if (cameraHolder != null)
        {
            cameraHolder.localRotation =
                Quaternion.Euler(
                    cameraPitch,
                    0f,
                    0f
                );
        }
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

        float currentJumpHeight =
            (jumpHeight + jumpBonus) *
            jumpBuffMultiplier;

        verticalVelocity = Mathf.Sqrt(
            currentJumpHeight *
            -2f *
            gravity
        );
    }

    private void ApplyGravity()
    {
        if (isGrounded &&
            verticalVelocity < 0f)
        {
            verticalVelocity =
                groundedVelocity;
        }

        verticalVelocity +=
            gravity *
            Time.deltaTime;

        characterController.Move(
            Vector3.up *
            verticalVelocity *
            Time.deltaTime
        );
    }

    private void ApplyMomentum()
    {
        if (momentumVelocity.sqrMagnitude <=
            minimumMomentumSpeed *
            minimumMomentumSpeed)
        {
            momentumVelocity =
                Vector3.zero;

            return;
        }

        characterController.Move(
            momentumVelocity *
            Time.deltaTime
        );

        momentumVelocity =
            Vector3.MoveTowards(
                momentumVelocity,
                Vector3.zero,
                momentumDrag *
                Time.deltaTime
            );

        if (isGrounded)
        {
            momentumVelocity =
                Vector3.zero;
        }
    }

    /// <summary>
    /// フック解除時の速度を慣性として受け取る。
    /// </summary>
    public void SetHookMomentum(Vector3 releaseVelocity)
    {
        momentumVelocity =
            new Vector3(
                releaseVelocity.x,
                0f,
                releaseVelocity.z
            );

        verticalVelocity =
            releaseVelocity.y;
    }

    /// <summary>
    /// アイテムによる永続的な速度強化。
    /// </summary>
    public void SpeedUp(float amount)
    {
        if (amount <= 0f)
        {
            Debug.LogWarning(
                "速度上昇値は0より大きい値にしてください。"
            );

            return;
        }

        speedBonus += amount;

        Debug.Log(
            $"移動速度が{amount}上昇しました。" +
            $"現在の永続速度ボーナス：{speedBonus}"
        );
    }

    /// <summary>
    /// アイテムによる永続的なジャンプ強化。
    /// </summary>
    public void JumpUp(float amount)
    {
        if (amount <= 0f)
        {
            Debug.LogWarning(
                "ジャンプ上昇値は0より大きい値にしてください。"
            );

            return;
        }

        jumpBonus += amount;

        Debug.Log(
            $"ジャンプ力が{amount}上昇しました。" +
            $"現在の永続ジャンプボーナス：{jumpBonus}"
        );
    }

    /// <summary>
    /// イベントによる一時的な移動速度・ジャンプ力強化。
    /// </summary>
    public void SetMovementBuff(
        float speedMultiplier,
        float jumpMultiplier
    )
    {
        movementBuffMultiplier =
            Mathf.Max(
                0f,
                speedMultiplier
            );

        jumpBuffMultiplier =
            Mathf.Max(
                0f,
                jumpMultiplier
            );

        Debug.Log(
            $"一時バフ開始：" +
            $"移動速度×{movementBuffMultiplier}、" +
            $"ジャンプ力×{jumpBuffMultiplier}"
        );
    }

    /// <summary>
    /// イベントによる一時バフを解除する。
    /// </summary>
    public void ResetMovementBuff()
    {
        movementBuffMultiplier = 1f;
        jumpBuffMultiplier = 1f;

        Debug.Log(
            "移動速度とジャンプ力の一時バフを解除しました。"
        );
    }

    public float GetSpeedBonus()
    {
        return speedBonus;
    }

    public float GetJumpBonus()
    {
        return jumpBonus;
    }

    public float GetCurrentWalkSpeed()
    {
        return
            (walkSpeed + speedBonus) *
            movementBuffMultiplier;
    }

    public float GetCurrentSprintSpeed()
    {
        return
            (sprintSpeed + speedBonus) *
            movementBuffMultiplier;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
        {
            return;
        }

        Gizmos.color =
            isGrounded
                ? Color.green
                : Color.red;

        Gizmos.DrawWireSphere(
            groundCheck.position,
            groundCheckRadius
        );
    }
}