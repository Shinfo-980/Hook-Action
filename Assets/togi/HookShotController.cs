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

    [Header("フック解除後の慣性")]
    [SerializeField] private float momentumMultiplier = 1f;

    private float currentPullSpeed;

    private PlayerInput playerInput;
    private InputAction hookAction;

    private Vector3 hookTargetPosition;
    private Vector3 playerTargetPosition;

    // 最後のフレームでPlayerが移動していた速度
    private Vector3 lastPullVelocity;

    private bool isHookFlying;
    private bool isHookAttached;
    private bool isPlayerPulling;

    public bool IsPlayerPulling => isPlayerPulling;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        hookAction = playerInput.actions["Hook"];

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

        ropeLine.positionCount = 2;

        ResetHook();
    }

    private void OnEnable()
    {
        hookAction.Enable();
    }

    private void OnDisable()
    {
        hookAction.Disable();
    }

    private void Update()
    {
        HandleHookInput();

        if (isHookFlying)
        {
            MoveHookTip();
        }

        if (isHookAttached)
        {
            if (hookAction.IsPressed())
            {
                isPlayerPulling = true;
                PullPlayer();
            }
            else
            {
                // 引っ張っている途中で離した場合だけ
                // 慣性をPlayerへ渡す
                ReleaseHook();
            }
        }

        if (isHookFlying || isHookAttached)
        {
            UpdateRope();
        }
    }

    private void HandleHookInput()
    {
        if (!hookAction.WasPressedThisFrame())
        {
            return;
        }

        if (!isHookFlying &&
            !isHookAttached &&
            !isPlayerPulling)
        {
            FireHook();
        }
    }

    private void FireHook()
    {
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
            Debug.Log("フックを刺せる場所がない");
            return;
        }

        hookTargetPosition = hit.point;

        playerTargetPosition =
            hit.point +
            hit.normal * stopDistanceFromWall;

        hookTip.position = hookOrigin.position;
        hookTip.rotation = hookOrigin.rotation;

        hookTip.SetParent(null, true);

        isHookFlying = true;
        isHookAttached = false;
        isPlayerPulling = false;

        currentPullSpeed = initialPullSpeed;
        lastPullVelocity = Vector3.zero;

        ropeLine.enabled = true;

        UpdateRope();

        Debug.Log("フック発射");
    }

    private void MoveHookTip()
    {
        hookTip.position = Vector3.MoveTowards(
            hookTip.position,
            hookTargetPosition,
            hookSpeed * Time.deltaTime
        );

        float distance = Vector3.Distance(
            hookTip.position,
            hookTargetPosition
        );

        if (distance <= hookArrivalDistance)
        {
            hookTip.position = hookTargetPosition;

            isHookFlying = false;
            isHookAttached = true;
            isPlayerPulling = false;

            Debug.Log("フックが壁に到着");
        }
    }

    private void PullPlayer()
    {
        currentPullSpeed +=
            pullAcceleration * Time.deltaTime;

        currentPullSpeed = Mathf.Min(
            currentPullSpeed,
            maxPullSpeed
        );

        Vector3 currentPosition =
            transform.position;

        Vector3 movement = Vector3.MoveTowards(
            currentPosition,
            playerTargetPosition,
            currentPullSpeed * Time.deltaTime
        ) - currentPosition;

        characterController.Move(movement);

        // 今回のフレームで実際に移動した速度を記録
        if (Time.deltaTime > 0f)
        {
            lastPullVelocity =
                movement / Time.deltaTime;
        }

        float distance = Vector3.Distance(
            transform.position,
            playerTargetPosition
        );

        if (distance <= playerArrivalDistance)
        {
            Debug.Log("Player引き寄せ完了");

            // 壁まで到着した場合は慣性を付けずに終了
            ResetHook();
        }
    }

    private void ReleaseHook()
    {
        bool wasPulling = isPlayerPulling;

        isPlayerPulling = false;

        if (wasPulling &&
            playerMovement != null)
        {
            playerMovement.SetHookMomentum(
                lastPullVelocity *
                momentumMultiplier
            );
        }

        ResetHook();
    }

    private void UpdateRope()
    {
        ropeLine.SetPosition(
            0,
            hookOrigin.position
        );

        ropeLine.SetPosition(
            1,
            hookTip.position
        );
    }

    private void ResetHook()
    {
        isHookFlying = false;
        isHookAttached = false;
        isPlayerPulling = false;

        currentPullSpeed = initialPullSpeed;
        lastPullVelocity = Vector3.zero;

        hookTip.SetParent(hookOrigin);

        hookTip.localPosition = Vector3.zero;
        hookTip.localRotation =
            Quaternion.identity;

        if (ropeLine != null)
        {
            ropeLine.enabled = false;
        }
    }
}