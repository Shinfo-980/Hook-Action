using UnityEngine;
using UnityEngine.InputSystem;

public class HookShotController : MonoBehaviour
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
    [SerializeField] private float initialPullSpeed = 8f;
    [SerializeField] private float maxPullSpeed = 30f;
    [SerializeField] private float pullAcceleration = 12f;
    [SerializeField] private float stopDistanceFromWall = 1.2f;
    [SerializeField] private float playerArrivalDistance = 0.1f;

    private float currentPullSpeed;

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

        /*
         * フックが壁に刺さっている間、
         * 左クリックを押している場合だけPlayerを引っ張る
         */
        if (isHookAttached)
        {
            if (hookAction.IsPressed())
            {
                isPlayerPulling = true;
                PullPlayer();
            }
            else
            {
                // 左クリックを離したら、その場で引き寄せ終了
                isPlayerPulling = false;
                ResetHook();
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

        // フックが何もしていないときだけ発射できる
        if (!isHookFlying &&
            !isHookAttached &&
            !isPlayerPulling)
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

        hookTargetPosition = hit.point;

        playerTargetPosition =
            hit.point + hit.normal * stopDistanceFromWall;

        hookTip.position = hookOrigin.position;
        hookTip.rotation = hookOrigin.rotation;

        hookTip.SetParent(null, true);

        isHookFlying = true;
        isHookAttached = false;
        isPlayerPulling = false;

        // 引き寄せ速度を初期化
        currentPullSpeed = initialPullSpeed;

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

            /*
             * ここではisPlayerPullingをtrueにしない。
             * 左クリックが押されているかどうかは
             * Update内で判定する。
             */
            isPlayerPulling = false;

            Debug.Log("フックが壁に到着");
        }
    }

    private void PullPlayer()
    {
        // 長押ししている間、徐々に加速する
        currentPullSpeed += pullAcceleration * Time.deltaTime;

        // 最大速度を超えないようにする
        currentPullSpeed = Mathf.Min(
            currentPullSpeed,
            maxPullSpeed
        );

        Vector3 currentPosition = transform.position;

        Vector3 movement = Vector3.MoveTowards(
            currentPosition,
            playerTargetPosition,
            currentPullSpeed * Time.deltaTime
        ) - currentPosition;

        characterController.Move(movement);

        float distance = Vector3.Distance(
            transform.position,
            playerTargetPosition
        );

        if (distance <= playerArrivalDistance)
        {
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

        // 次のフック用に速度を初期化
        currentPullSpeed = initialPullSpeed;

        hookTip.SetParent(hookOrigin);

        hookTip.localPosition = Vector3.zero;
        hookTip.localRotation = Quaternion.identity;

        if (ropeLine != null)
        {
            ropeLine.enabled = false;
        }
    }
}