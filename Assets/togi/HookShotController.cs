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

    [Header("SE用AudioSource")]
    [Tooltip("発射音と命中音を再生するAudioSource")]
    [SerializeField] private AudioSource oneShotAudioSource;

    [Tooltip("飛行音と引き寄せ音を再生するAudioSource")]
    [SerializeField] private AudioSource loopAudioSource;

    [Header("フック発射SE")]
    [SerializeField] private AudioClip hookShotSE;

    [Range(0f, 1f)]
    [SerializeField] private float hookShotSEVolume = 1f;

    [Header("フック飛行中SE")]
    [Tooltip("フックが飛んでいる間にループ再生するSE")]
    [SerializeField] private AudioClip hookFlyingLoopSE;

    [Range(0f, 1f)]
    [SerializeField] private float hookFlyingLoopSEVolume = 1f;

    [Header("フック命中SE")]
    [Tooltip("フックが壁に引っかかった瞬間のSE")]
    [SerializeField] private AudioClip hookAttachSE;

    [Range(0f, 1f)]
    [SerializeField] private float hookAttachSEVolume = 1f;

    [Header("Player引き寄せ中SE")]
    [Tooltip("フックでPlayerを引き寄せている間にループ再生するSE")]
    [SerializeField] private AudioClip hookPullLoopSE;

    [Range(0f, 1f)]
    [SerializeField] private float hookPullLoopSEVolume = 1f;

    private PlayerInput playerInput;
    private InputAction hookFireAction;
    private InputAction moveAction;

    private Vector3 hookTargetPosition;
    private Vector3 playerTargetPosition;

    private Vector3 lastPullVelocity;

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
                "HookShotControllerと同じGameObjectにPlayerInputがありません。"
            );

            enabled = false;
            return;
        }

        hookFireAction =
            playerInput.actions.FindAction(
                "HookFire",
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

        SetupAudioSources();
        ResetHook();
    }

    private void OnEnable()
    {
        hookFireAction?.Enable();
        moveAction?.Enable();
    }

    private void OnDisable()
    {
        hookFireAction?.Disable();
        moveAction?.Disable();

        ResetHook();
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
            if (hookFireAction != null &&
                hookFireAction.IsPressed())
            {
                if (!isPlayerPulling)
                {
                    isPlayerPulling = true;
                    PlayHookPullLoopSE();
                }

                PullPlayer();
            }
            else
            {
                if (isPlayerPulling &&
                    playerMovement != null)
                {
                    playerMovement.SetHookMomentum(
                        lastPullVelocity *
                        momentumMultiplier
                    );
                }

                ResetHook();
            }
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

        if (isPlayerPulling)
        {
            UpdateRopeLengthWhilePulling();
        }

        LimitRopeLength();
        UpdateRope();
    }

    private void SetupAudioSources()
    {
        AudioSource[] audioSources =
            GetComponents<AudioSource>();

        if (oneShotAudioSource == null &&
            audioSources.Length > 0)
        {
            oneShotAudioSource =
                audioSources[0];
        }

        if (loopAudioSource == null)
        {
            if (audioSources.Length > 1)
            {
                loopAudioSource =
                    audioSources[1];
            }
            else
            {
                loopAudioSource =
                    gameObject.AddComponent<AudioSource>();
            }
        }

        if (oneShotAudioSource == null)
        {
            oneShotAudioSource =
                gameObject.AddComponent<AudioSource>();
        }

        oneShotAudioSource.playOnAwake = false;
        oneShotAudioSource.loop = false;

        loopAudioSource.playOnAwake = false;
        loopAudioSource.loop = true;
    }

    private void HandleHookFireInput()
    {
        if (hookFireAction == null ||
            !hookFireAction.WasPressedThisFrame())
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

    private void HandleHookFireRelease()
    {
        if (hookFireAction == null ||
            !hookFireAction.WasReleasedThisFrame())
        {
            return;
        }

        if (!isHookFlying &&
            !isHookAttached &&
            !isPlayerPulling)
        {
            return;
        }

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
            "フックボタンを離したためフック解除"
        );
    }

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
                "Hook OriginまたはHook Tipが設定されていません。"
            );

            return;
        }

        Vector3 rayOrigin =
            playerCamera.transform.position;

        Vector3 rayDirection =
            playerCamera.transform.forward;

        bool hitSomething =
            Physics.Raycast(
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

        playerTargetPosition =
            hit.point +
            hit.normal *
            stopDistanceFromWall;

        hookTip.position =
            hookOrigin.position;

        hookTip.rotation =
            hookOrigin.rotation;

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

        PlayHookShotSE();
        PlayHookFlyingLoopSE();

        Debug.Log(
            "フック発射"
        );
    }

    private void MoveHookTip()
    {
        if (hookTip == null)
        {
            ResetHook();
            return;
        }

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

        attachedRopeLength =
            Vector3.Distance(
                hookOrigin.position,
                hookTargetPosition
            );

        StopLoopSE();
        PlayHookAttachSE();

        Debug.Log(
            "フックが壁に引っかかりました。"
        );
    }

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

        if (distance <=
            playerArrivalDistance)
        {
            lastPullVelocity =
                Vector3.zero;

            StopLoopSE();

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

        Vector3 pullMovement =
            Vector3.MoveTowards(
                currentPosition,
                playerTargetPosition,
                currentPullSpeed *
                Time.deltaTime
            ) - currentPosition;

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

    private void UpdateRopeLengthWhilePulling()
    {
        if (hookOrigin == null)
        {
            return;
        }

        float currentRopeLength =
            Vector3.Distance(
                hookOrigin.position,
                hookTargetPosition
            );

        attachedRopeLength =
            Mathf.Min(
                attachedRopeLength,
                currentRopeLength
            );
    }

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

        moveDirection.y = 0f;

        if (moveDirection.sqrMagnitude > 1f)
        {
            moveDirection.Normalize();
        }

        return moveDirection *
               hookMoveSpeed;
    }

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

    private void ResetHook()
    {
        StopLoopSE();

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
            ropeLine.enabled = false;
        }
    }

    private void PlayHookShotSE()
    {
        PlayOneShotSE(
            hookShotSE,
            hookShotSEVolume,
            "Hook Shot SE"
        );
    }

    private void PlayHookAttachSE()
    {
        PlayOneShotSE(
            hookAttachSE,
            hookAttachSEVolume,
            "Hook Attach SE"
        );
    }

    private void PlayHookFlyingLoopSE()
    {
        PlayLoopSE(
            hookFlyingLoopSE,
            hookFlyingLoopSEVolume,
            "Hook Flying Loop SE"
        );
    }

    private void PlayHookPullLoopSE()
    {
        PlayLoopSE(
            hookPullLoopSE,
            hookPullLoopSEVolume,
            "Hook Pull Loop SE"
        );
    }

    private void PlayOneShotSE(
        AudioClip audioClip,
        float volume,
        string seName
    )
    {
        if (oneShotAudioSource == null)
        {
            Debug.LogWarning(
                "One Shot Audio Sourceが設定されていません。"
            );

            return;
        }

        if (audioClip == null)
        {
            Debug.LogWarning(
                $"{seName}が設定されていません。"
            );

            return;
        }

        oneShotAudioSource.PlayOneShot(
            audioClip,
            volume
        );
    }

    private void PlayLoopSE(
        AudioClip audioClip,
        float volume,
        string seName
    )
    {
        if (loopAudioSource == null)
        {
            Debug.LogWarning(
                "Loop Audio Sourceが設定されていません。"
            );

            return;
        }

        if (audioClip == null)
        {
            Debug.LogWarning(
                $"{seName}が設定されていません。"
            );

            return;
        }

        if (loopAudioSource.isPlaying &&
            loopAudioSource.clip == audioClip)
        {
            return;
        }

        loopAudioSource.Stop();

        loopAudioSource.clip =
            audioClip;

        loopAudioSource.volume =
            volume;

        loopAudioSource.loop =
            true;

        loopAudioSource.Play();
    }

    private void StopLoopSE()
    {
        if (loopAudioSource == null)
        {
            return;
        }

        loopAudioSource.Stop();
        loopAudioSource.clip = null;
    }
}