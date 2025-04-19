using Cinemachine;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStateMachine : MonoBehaviour
{
    public PlayerState currentState;
    public PlayerStateContext context = new PlayerStateContext();

    [Header("States")]
    public PlayerState motionState;
    public PlayerState lockedOnMotionState;
    public PlayerState jumpState;
    public PlayerState rollState;
    public PlayerState attackState;
    public PlayerState dodgeState;
    public PlayerState jumpAttackState;
    public PlayerState heavyAttackState;
    public PlayerState dodgeAttackState;

    [Header("Camera")]
    public CinemachineFreeLook playerCamera;
    public CinemachineVirtualCamera lockOnCamera;
    public Transform cameraTarget;
    public Transform cameraPivot;
    public float cameraSensitivity = 1.0f;
    float pitch = 0f;
    float yaw = 0f;
    float minVerticalAngle = 0f;
    float maxVerticalAngle = 40f;
    float lockOnDistanceBehindPlayer = 5f;
    float cameraSwapSpeed = 5f;
    bool hasResetToZero;
    Coroutine LockOnCoroutine;
    Coroutine unlockCoroutine;

    [Header("Physics")]
    public Transform groundCheckTransform;
    GroundCheck groundCheck;
    public Vector3 velocity;
    public float gravity = -15f;

    [Header("Movement")]
    public float movementSpeed = 4f;
    public float combatMovementSpeed = 3f;
    public float sprintSpeed = 6.6f;
    public float playerRotationSpeed = 9;

    [Header("Jumping")]
    public float jumpForce = 9f;

    [Header("Rolling")]
    public Vector3 rollDirection;
    public Vector3 rollLookDirection;
    public float rollSpeed = 15f;
    public float rollDuration;
    public float rollTimer;

    [Header("Dodging")]
    public Vector3 dodgeDirection;
    public float dodgeSpeed = 14;
    public float dodgeDuration;
    public float dodgeTimer;

    [Header("Attack")]
    public Weapon defaultWeapon;
    public float attackMovementToTargetThreshold = 2.2f;
    public float attackMovementSpeed = 20f;
    public float attackMovementDuration = 2f;
    public bool isAttackMoving;

    [Header("Jump Attack")]
    public float jumpAttackLift = 2f;
    public float jumpAttackSpeedModifier = 3f;
    public float groundDistanceAttackTrigger = 1f;

    [Header("Charged Attack")]

    [Header("DodgeAttack")]
    public bool dodgeAttackQueued;
    public float dodgeAttackMovementSpeed = 12f;
    public float rollAttackMovementSpeed = 7f;
    public float dodgeAttackMovementDuration = 0.5f;

    private Coroutine attackMovementRoutine;


    [HideInInspector] public Animator animator;
    [HideInInspector] public CharacterController controller;
    [HideInInspector] public PlayerInputHandler input;
    [HideInInspector] public WeaponController weaponController;


    private void Awake()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        input = GetComponent<PlayerInputHandler>();
        weaponController = GetComponent<WeaponController>();
        groundCheck = groundCheckTransform.GetComponent<GroundCheck>();
        GetComponent<AnimationEventForwarder>().stateMachine = this;
    }

    private void Start()
    {
        currentState.OnEnter(this);
        rollDuration = GetClipLength("Roll00_1_RollAction***");
        context.lockOnDistance = GetComponent<CombatController>().sightDistance;

        weaponController.EquipWeapon(defaultWeapon);
        context.currentWeapon = weaponController.currentWeapon;
    }

    void Update()
    {
        currentState.OnUpdate(this);
        HandleContextComponents();
        CameraMovement();
        DownwardVelocitySetter();
        LockOnHandler();
    }

    public void SwitchState(PlayerState newState)
    {
        currentState.OnExit(this);
        currentState = newState;
        currentState.OnEnter(this);
    }

    private void DownwardVelocitySetter()
    {
        if (context.isGrounded && currentState != jumpState) { velocity.y = -2f; }
    }

    public Vector3 GetMovementDirection()
    {
        Quaternion orientation = context.isLockedOn ? lockOnCamera.State.FinalOrientation : playerCamera.State.FinalOrientation;

        Vector3 cameraForwardDirection = orientation * Vector3.forward;
        Vector3 cameraRightDirection = orientation * Vector3.right;

        cameraForwardDirection.y = 0f;
        cameraRightDirection.y = 0f;
        cameraForwardDirection.Normalize();
        cameraRightDirection.Normalize();
        Vector2 moveInput = context.canMove ? input.moveInput : Vector2.zero;
        Vector3 movementDirection = cameraRightDirection * moveInput.x + cameraForwardDirection * moveInput.y;
        return movementDirection;
    }

    public Vector3 PlayerToObjectVector(GameObject gameObject)
    {
        return gameObject.transform.position - transform.position;
    }

    public float PlayerToObjectDistance(GameObject gameObject)
    {
        return PlayerToObjectVector(gameObject).magnitude;
    }

    private void HandleContextComponents()
    {
        if (input.sprintTogglePressed && context.canToggleSprint)
        {
            context.isSprinting = !context.isSprinting;
        }
        context.isGrounded = groundCheck.grounded;
    }

    private void CameraMovement()
    {
        Vector2 cameraMoveInput = input.cameraMoveInput;
        pitch -= cameraMoveInput.y * cameraSensitivity;
        pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);

        if (!context.isLockedOn)
        {
            lockOnCamera.Priority = 10;
            playerCamera.Priority = 20;

            if (!context.isLockingOn) { yaw += cameraMoveInput.x * cameraSensitivity; }

            cameraPivot.localRotation = Quaternion.Euler(0f, yaw, 0f);
            cameraTarget.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }
        else
        {
            lockOnCamera.Priority = 20;
            playerCamera.Priority = 10;

            cameraPivot.rotation = transform.rotation;

            SetLockOnCameraPositionAndRotation();

            if (JoystickMovedThisFrame(cameraMoveInput))
            {
                CombatTarget newTarget = GetComponent<CombatController>().SwitchTarget(context.target, cameraMoveInput);
                context.target = newTarget;
            }
        }

        if (context.isLockedOn && !context.wasLockedOn)
        {

            if (unlockCoroutine != null) { StopCoroutine(unlockCoroutine); }
            LockOnCoroutine = StartCoroutine(CameraLockOnDelay());
            StartCoroutine(LockingOn());
        }
        else if (!context.isLockedOn && context.wasLockedOn)
        {
            if (lockOnCamera != null) { StopCoroutine(LockOnCoroutine); }
            unlockCoroutine = StartCoroutine(CameraUnlockDelay());
            StartCoroutine(LockingOn());
        }
        context.wasLockedOn = context.isLockedOn;
    }

    private void SetLockOnCameraPositionAndRotation()
    {
        if (context.target == null) { return; }
        Vector3 playerToTargetVector = context.target.transform.position - transform.position;
        playerToTargetVector.y = -1.2f;
        playerToTargetVector.Normalize();

        Quaternion lookRotation = Quaternion.LookRotation(playerToTargetVector, Vector3.up);
        Vector3 cameraOffset = playerToTargetVector * lockOnDistanceBehindPlayer - Vector3.up * 2;
        Vector3 cameraPosition = transform.position - cameraOffset;

        lockOnCamera.transform.position = Vector3.Lerp(
            lockOnCamera.transform.position,
            cameraPosition,
            cameraSwapSpeed * Time.deltaTime);

        lockOnCamera.transform.rotation = Quaternion.Slerp(
            lockOnCamera.transform.rotation,
            lookRotation,
            cameraSwapSpeed * Time.deltaTime);
    }

    private bool JoystickMovedThisFrame(Vector2 input)
    {
        float threshold = 0.1f;
        bool joystickIsMoving = input.magnitude > threshold;
        bool joystickWasMovedThisFrame = false;

        if (joystickIsMoving && hasResetToZero)
        {
            joystickWasMovedThisFrame = true;
            hasResetToZero = false;
        }

        if (!joystickIsMoving) { hasResetToZero = true; }
        return joystickWasMovedThisFrame;
    }

    private IEnumerator CameraLockOnDelay()
    {
        yield return new WaitForSeconds(1.2f);

        playerCamera.Follow = lockOnCamera.transform;

        for (int i = 0; i < 3; i++)
        {
            playerCamera.m_Orbits[i].m_Radius = 0f;
            playerCamera.m_Orbits[i].m_Height = 0f;
        }

        yield return new WaitForSeconds(3f);
        context.isLockingOn = false;
    }

    private IEnumerator CameraUnlockDelay()
    {
        yield return new WaitForSeconds(.65f);

        playerCamera.Follow = cameraTarget;
        playerCamera.LookAt = cameraTarget;

        playerCamera.m_Orbits[0].m_Height = 5f;
        playerCamera.m_Orbits[1].m_Height = 1.5f;
        playerCamera.m_Orbits[2].m_Height = 0f;
        playerCamera.m_Orbits[0].m_Radius = 2f;
        playerCamera.m_Orbits[1].m_Radius = 4f;
        playerCamera.m_Orbits[2].m_Radius = 4.5f;
    }

    private IEnumerator LockingOn()
    {
        context.isLockingOn = true;
        yield return new WaitForSeconds(4f);
        context.isLockingOn = false;
    }

    private void LockOnHandler()
    {
        if (input.lockOnTogglePressed && context.canToggleLockOn) { LockOn(); }
        ControlLockOn();
    }

    private void LockOn()
    {
        if (context.target == null) { context.target = GetComponent<CombatController>().LockOn(); }
        else { context.target = null; }
    }

    private void ControlLockOn()
    {
        if (context.target == null) { context.isLockedOn = false; }
        else
        {
            context.isLockedOn = true;
            float distance = PlayerToObjectDistance(context.target.gameObject);
            if (distance > context.lockOnDistance) { context.target = null; }
        }
    }

    public Vector2 HardInputCalculator()
    {
        Vector2 currentInput = input.moveInput;
        float horizontalInput = currentInput.x;
        float verticalInput = currentInput.y;
        float threshold = 0.2f;

        float horizontal = Mathf.Abs(horizontalInput) >= threshold ? Mathf.Sign(horizontalInput) : 0f;
        float vertical = Mathf.Abs(verticalInput) >= threshold ? Mathf.Sign(verticalInput) : 0f;
        return new Vector2(horizontal, vertical);
    }

    public float GetClipLength(string clipName)
    {
        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipName)
            {
                return clip.length;
            }
        }
        return 0.5f;
    }

    public float GetCurrentClipLength()
    {
        AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);

        if (clipInfo.Length == 0)
        {
            return 0f;
        }
        AnimationClip currentClip = clipInfo[0].clip;
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        return currentClip.length / stateInfo.speed;
    }

    public Weapon SetWeapon(Weapon weapon)
    {
        return weapon;
    }

    public float DistanceToGround()
    {
        RaycastHit hit;
        if (Physics.Raycast(groundCheckTransform.position, Vector3.down, out hit))
        {
            return hit.distance;
        }
        return Mathf.Infinity;
    }

    public void StartAttackMovment(PlayerStateMachine player, float duration, float speed)
    {
        if (attackMovementRoutine != null)
        {
            StopCoroutine(attackMovementRoutine);
        }
        attackMovementRoutine = StartCoroutine(AttackMovement(player, duration, speed));
    }

    public IEnumerator AttackMovement(PlayerStateMachine player, float duration, float speed)
    {
        player.isAttackMoving = true;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float movementSpeed = Mathf.Lerp(speed, 0f, elapsed / duration);
            Vector3 movement = player.transform.forward * movementSpeed * Time.deltaTime;
            movement.y = -2f;

            player.controller.Move(movement);
            elapsed += Time.deltaTime;

            yield return null;
        }
        player.isAttackMoving = false;
    }
}
