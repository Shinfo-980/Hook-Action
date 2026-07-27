using UnityEngine;
using UnityEngine.InputSystem;

public class HookShotTest: MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private CharacterController characterController;
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
    [SerializeField] private float pullSpeed = 15f;

    // 壁からどの程度手前で停止するか
    [SerializeField] private float stopDistanceFromWall = 1.2f;

    // 目的地にどこまで近づいたら停止扱いにするか
    [SerializeField] private float playerArrivalDistance = 0.1f;

    private PlayerInput playerInput;
    private InputAction hookAction;

    private Vector3 hookTargetPosition;
    private Vector3 playerTargetPosition;

    private bool isHookFlying;
    private bool isHookAttached;
    private bool isPlayerPulling;

    public bool IsPlayerPulling => isPlayerPulling;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        hookAction = playerInput.actions["Hook"];

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

        if (isPlayerPulling)
        {
            PullPlayer();
        }

        if (isHookFlying || isHookAttached || isPlayerPulling)
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

        // フックが何もしていないときだけ発射
        if (!isHookFlying && !isHookAttached && !isPlayerPulling)
        {
            FireHook();
        }
    }

    private void FireHook()
    {
        Vector3 rayOrigin = playerCamera.transform.position;
        Vector3 rayDirection = playerCamera.transform.forward;

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

        // フック先端が向かう位置
        hookTargetPosition = hit.point;

        // Playerが最終的に向かう位置
        // 壁の表面から法線方向へ少し離れた場所にする
        playerTargetPosition =
            hit.point + hit.normal * stopDistanceFromWall;

        // 発射時にHookTipを銃口位置へ合わせる
        hookTip.position = hookOrigin.position;
        hookTip.rotation = hookOrigin.rotation;

        // Playerやカメラの動きから独立させる
        hookTip.SetParent(null, true);

        isHookFlying = true;
        isHookAttached = false;
        isPlayerPulling = false;

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
            isPlayerPulling = true;

            Debug.Log("フックが壁に到着");
            Debug.Log("Player引き寄せ開始");
        }
    }

    private void PullPlayer()
    {
        Vector3 currentPosition = transform.position;

        Vector3 movement = Vector3.MoveTowards(
            currentPosition,
            playerTargetPosition,
            pullSpeed * Time.deltaTime
        ) - currentPosition;

        characterController.Move(movement);

        float distance = Vector3.Distance(
            transform.position,
            playerTargetPosition
        );

        if (distance <= playerArrivalDistance)
        {
            isPlayerPulling = false;

            Debug.Log("Player引き寄せ完了");

            ResetHook();
        }
    }

    private void UpdateRope()
    {
        ropeLine.SetPosition(0, hookOrigin.position);
        ropeLine.SetPosition(1, hookTip.position);
    }

    private void ResetHook()
    {
        isHookFlying = false;
        isHookAttached = false;
        isPlayerPulling = false;

        // HookTipをHookOriginの子に戻す
        hookTip.SetParent(hookOrigin);

        hookTip.localPosition = Vector3.zero;
        hookTip.localRotation = Quaternion.identity;

        if (ropeLine != null)
        {
            ropeLine.enabled = false;
        }
    }
}