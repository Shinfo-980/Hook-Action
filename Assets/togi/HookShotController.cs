using UnityEngine;
using UnityEngine.InputSystem;

public class HookShotController : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Transform hookOrigin;
    [SerializeField] private Transform hookTip;
    [SerializeField] private LineRenderer ropeLine;

    [Header("Raycast設定")]
    [SerializeField] private float maxHookDistance = 30f;
    [SerializeField] private LayerMask hookableLayer;

    [Header("フック移動設定")]
    [SerializeField] private float hookSpeed = 40f;
    [SerializeField] private float hookArrivalDistance = 0.02f;

    [Header("Player引き寄せ設定")]
    [SerializeField] private float initialPullSpeed = 8f;
    [SerializeField] private float maxPullSpeed = 30f;
    [SerializeField] private float pullAcceleration = 12f;
    [SerializeField] private float stopDistanceFromWall = 1.2f;
    [SerializeField] private float playerArrivalDistance = 0.1f;

    [Header("フック中の移動操作")]
    [Tooltip("フック移動中にWASDや左スティックで操作できる速度")]
    [SerializeField] private float hookMoveSpeed = 4f;

    [Tooltip("左スティックの微小入力を無視する値")]
    [Range(0f, 1f)]
    [SerializeField] private float moveDeadZone = 0.1f;

    [Header("フック解除後の慣性")]
    [SerializeField] private float momentumMultiplier = 1f;

    private PlayerInput playerInput;

    // ZR：フック発射・接続維持
    private InputAction hookFireAction;

    // R：Player引き寄せ
    private InputAction hookPullAction;

    // WASD・左スティック
    private InputAction moveAction;

    private Vector3 hookTargetPosition;
    private Vector3 playerTargetPosition;

    // フック解除時にPlayerMovementへ渡す速度
    private Vector3 lastPullVelocity;

    // フックが刺さった瞬間のロープの長さ
    private float attachedRopeLength;

    private float currentPullSpeed;

    private bool isHookFlying;
    private bool isHookAttached;
    private bool isPlayerPulling;

    public bool IsHookFlying => isHookFlying;
    public bool IsHookAttached => isHookAttached;
    public bool IsPlayerPulling => isPlayerPulling;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        if (playerInput == null)
        {
            Debug.LogError(
                "HookShotControllerと同じGameObjectに" +
                "PlayerInputがありません。"
            );

            enabled = false;
            return;
        }

        hookFireAction =
            playerInput.actions.FindAction(
                "HookFire",
                true
            );

        hookPullAction =
            playerInput.actions.FindAction(
                "HookPull",
                true
            );

        moveAction =
            playerInput.actions.FindAction(
                "Move",
                true
            );

        if (characterController == null)
        {
            characterController =
                GetComponent<CharacterController>();
        }

        if (playerMovement == null)
        {
            playerMovement =
                GetComponent<PlayerMovement>();
        }

        if (ropeLine != null)
        {
            ropeLine.positionCount = 2;
        }

        ResetHook();
    }

    private void OnEnable()
    {
        hookFireAction?.Enable();
        hookPullAction?.Enable();
        moveAction?.Enable();
    }

    private void OnDisable()
    {
        hookFireAction?.Disable();
        hookPullAction?.Disable();
        moveAction?.Disable();
    }

    private void Update()
    {
        HandleHookFireInput();
        HandleHookFireRelease();

        if (isHookFlying)
        {
            MoveHookTip();
        }

        if (isHookAttached)
        {
            HandleHookPullInput();
        }

        if (isHookFlying ||
            isHookAttached)
        {
            UpdateRope();
        }
    }

    private void LateUpdate()
    {
        if (!isHookAttached)
        {
            return;
        }

        LimitRopeLength();
        UpdateRope();
    }

    /// <summary>
    /// ZRを押した瞬間にフックを発射する
    /// </summary>
    private void HandleHookFireInput()
    {
        if (!hookFireAction.WasPressedThisFrame())
        {
            return;
        }

        if (isHookFlying ||
            isHookAttached ||
            isPlayerPulling)
        {
            return;
        }

        FireHook();
    }

    /// <summary>
    /// ZRを離したときだけフックを解除する
    /// </summary>
    private void HandleHookFireRelease()
    {
        if (!hookFireAction.WasReleasedThisFrame())
        {
            return;
        }

        if (!isHookFlying &&
            !isHookAttached &&
            !isPlayerPulling)
        {
            return;
        }

        // 巻き取り中にZRを離した場合は
        // 最後の速度を慣性として渡す
        if (isPlayerPulling &&
            playerMovement != null)
        {
            playerMovement.SetHookMomentum(
                lastPullVelocity *
                momentumMultiplier
            );
        }

        ResetHook();

        Debug.Log(
            "ZRを離したためフック解除"
        );
    }

    /// <summary>
    /// Rを押している間だけPlayerを引き寄せる
    /// Rを離してもフックは解除しない
    /// </summary>
    private void HandleHookPullInput()
    {
        if (hookPullAction.IsPressed())
        {
            isPlayerPulling = true;

            PullPlayer();

            return;
        }

        // Rを離したら巻き取りだけ停止する
        if (isPlayerPulling)
        {
            isPlayerPulling = false;

            currentPullSpeed =
                initialPullSpeed;

            lastPullVelocity =
                Vector3.zero;

            Debug.Log(
                "巻き取り停止"
            );
        }
    }

    /// <summary>
    /// フックを発射する
    /// </summary>
    private void FireHook()
    {
        if (playerCamera == null)
        {
            Debug.LogError(
                "Player Cameraが設定されていません。"
            );

            return;
        }

        if (hookOrigin == null ||
            hookTip == null)
        {
            Debug.LogError(
                "Hook OriginまたはHook Tipが" +
                "設定されていません。"
            );

            return;
        }

        Vector3 rayOrigin =
            playerCamera.transform.position;

        Vector3 rayDirection =
            playerCamera.transform.forward;

        bool hitSomething = Physics.Raycast(
            rayOrigin,
            rayDirection,
            out RaycastHit hit,
            maxHookDistance,
            hookableLayer,
            QueryTriggerInteraction.Ignore
        );

        if (!hitSomething)
        {
            Debug.Log(
                "フックを刺せる場所がありません。"
            );

            return;
        }

        hookTargetPosition =
            hit.point;

        // Playerが壁にめり込まない位置
        playerTargetPosition =
            hit.point +
            hit.normal *
            stopDistanceFromWall;

        hookTip.position =
            hookOrigin.position;

        hookTip.rotation =
            hookOrigin.rotation;

        // Playerの子のままだと一緒に動いてしまうため外す
        hookTip.SetParent(
            null,
            true
        );

        isHookFlying = true;
        isHookAttached = false;
        isPlayerPulling = false;

        currentPullSpeed =
            initialPullSpeed;

        lastPullVelocity =
            Vector3.zero;

        attachedRopeLength =
            0f;

        if (ropeLine != null)
        {
            ropeLine.enabled = true;
        }

        UpdateRope();

        Debug.Log(
            "フック発射"
        );
    }

    /// <summary>
    /// フック先端を命中地点まで移動させる
    /// </summary>
    private void MoveHookTip()
    {
        hookTip.position =
            Vector3.MoveTowards(
                hookTip.position,
                hookTargetPosition,
                hookSpeed *
                Time.deltaTime
            );

        float distance =
            Vector3.Distance(
                hookTip.position,
                hookTargetPosition
            );

        if (distance >
            hookArrivalDistance)
        {
            return;
        }

        hookTip.position =
            hookTargetPosition;

        isHookFlying = false;
        isHookAttached = true;
        isPlayerPulling = false;

        currentPullSpeed =
            initialPullSpeed;

        lastPullVelocity =
            Vector3.zero;

        // 刺さった瞬間のロープ長を保存
        attachedRopeLength =
            Vector3.Distance(
                hookOrigin.position,
                hookTargetPosition
            );

        Debug.Log(
            "フックが壁に到着しました。" +
            "Rを押すと巻き取ります。"
        );
    }

    /// <summary>
    /// Playerをフック地点へ引き寄せる
    /// </summary>
    private void PullPlayer()
    {
        if (characterController == null)
        {
            return;
        }

        float distance =
            Vector3.Distance(
                transform.position,
                playerTargetPosition
            );

        // 到着してもフックは解除しない
        // ZRを離すまで接続状態を維持する
        if (distance <=
            playerArrivalDistance)
        {
            lastPullVelocity =
                Vector3.zero;

            return;
        }

        currentPullSpeed +=
            pullAcceleration *
            Time.deltaTime;

        currentPullSpeed =
            Mathf.Min(
                currentPullSpeed,
                maxPullSpeed
            );

        Vector3 currentPosition =
            transform.position;

        // フックによる引き寄せ
        Vector3 pullMovement =
            Vector3.MoveTowards(
                currentPosition,
                playerTargetPosition,
                currentPullSpeed *
                Time.deltaTime
            ) - currentPosition;

        // Move入力による軌道修正
        Vector3 moveControl =
            CalculateMoveControl();

        Vector3 controlMovement =
            moveControl *
            Time.deltaTime;

        Vector3 finalMovement =
            pullMovement +
            controlMovement;

        characterController.Move(
            finalMovement
        );

        if (Time.deltaTime > 0f)
        {
            lastPullVelocity =
                finalMovement /
                Time.deltaTime;
        }
    }

    /// <summary>
    /// フックが刺さった瞬間より
    /// ロープが長くならないようPlayerを補正する
    /// </summary>
    private void LimitRopeLength()
    {
        if (!isHookAttached ||
            characterController == null ||
            hookOrigin == null)
        {
            return;
        }

        if (attachedRopeLength <= 0f)
        {
            return;
        }

        Vector3 toHook =
            hookTargetPosition -
            hookOrigin.position;

        float currentRopeLength =
            toHook.magnitude;

        if (currentRopeLength <=
            attachedRopeLength)
        {
            return;
        }

        float excessLength =
            currentRopeLength -
            attachedRopeLength;

        Vector3 correctionDirection =
            toHook.normalized;

        characterController.Move(
            correctionDirection *
            excessLength
        );
    }

    /// <summary>
    /// フック移動中のMove入力を計算する
    /// </summary>
    private Vector3 CalculateMoveControl()
    {
        if (moveAction == null)
        {
            return Vector3.zero;
        }

        Vector2 moveInput =
            moveAction.ReadValue<Vector2>();

        if (moveInput.sqrMagnitude <
            moveDeadZone *
            moveDeadZone)
        {
            return Vector3.zero;
        }

        moveInput =
            Vector2.ClampMagnitude(
                moveInput,
                1f
            );

        Vector3 moveDirection =
            transform.right *
            moveInput.x +
            transform.forward *
            moveInput.y;

        // フック中の操作では上下移動を加えない
        moveDirection.y = 0f;

        if (moveDirection.sqrMagnitude > 1f)
        {
            moveDirection.Normalize();
        }

        return moveDirection *
               hookMoveSpeed;
    }

    /// <summary>
    /// ロープ描画を更新する
    /// </summary>
    private void UpdateRope()
    {
        if (ropeLine == null ||
            hookOrigin == null ||
            hookTip == null)
        {
            return;
        }

        ropeLine.SetPosition(
            0,
            hookOrigin.position
        );

        ropeLine.SetPosition(
            1,
            hookTip.position
        );
    }

    /// <summary>
    /// フックを初期状態へ戻す
    /// </summary>
    private void ResetHook()
    {
        isHookFlying = false;
        isHookAttached = false;
        isPlayerPulling = false;

        currentPullSpeed =
            initialPullSpeed;

        lastPullVelocity =
            Vector3.zero;

        attachedRopeLength =
            0f;

        if (hookTip != null &&
            hookOrigin != null)
        {
            hookTip.SetParent(
                hookOrigin
            );

            hookTip.localPosition =
                Vector3.zero;

            hookTip.localRotation =
                Quaternion.identity;
        }

        if (ropeLine != null)
        {
            ropeLine.enabled =
                false;
        }
    }
}