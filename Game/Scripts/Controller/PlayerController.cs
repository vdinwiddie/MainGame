using UnityEngine;
using Cinemachine;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class PlayerController : MonoBehaviour
{
    [Header("Controls")]
    CharacterController controller;
    PlayerInputHandler inputHandler;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckTransform;
    GroundCheck groundCheck;

    [Header("Player Movement")]
    [SerializeField] float movementSpeed = 4f;
    [SerializeField] float sprintSpeed = 7f;
    [SerializeField] float playerRotationSpeed = 9f;
    [SerializeField] float combatMovementSpeed = 3.2f;

    Vector3 velocity;
    Vector3 lastPosition;

    bool movementDisabled;
    bool isMoving;
    bool sprinting;

    [Header("Camera Movement")]
    [SerializeField] CinemachineFreeLook playerCamera;
    [SerializeField] Transform cameraTarget;
    [SerializeField] Transform cameraPivot;
    [SerializeField] float cameraSensitivity = 1f;
    float yaw = 0f;
    float pitch = 0f;
    float minVerticalAngle = 0f;
    float maxVerticalAngle = 40f;
    float cameraSwapSpeed = 5f;

    bool hasResetToZero = true;

    [Header("Lock On")]
    [SerializeField] CinemachineVirtualCamera lockOnCamera;
    [SerializeField] float lockOnDistanceBehindPlayer = 5f;
    Coroutine lockOnCoroutine;
    Coroutine unlockCoroutine;

    bool lockedOn;
    bool wasLockedOn;
    bool lockingOn;

    [Header("Jump")]
    [SerializeField] float jumpForce = 8.3f;
    [SerializeField] float gravity = -15f;
    [SerializeField] float jumpLagTime = 0.05f;

    private bool canJump = true;
    private bool isJumping;
    private float canJumpDelay = 0.6f;

    [Header("Roll")]
    [SerializeField] float rollSpeed = 15f;
    Vector3 rollDirection;

    float rollAnimationLength = 1.4f;
    float rollLagTimeAnimationLength = 0.40f;
    float rollTimer;

    bool canRoll = true;
    bool isRolling;


    [Header("Dodge")]
    [SerializeField] private float dodgeSpeed = 7f;
    Vector2 currentDodgeInput = Vector2.zero;
    float dodgeAnimationLength = 0.5f;
    float dodgeLagTimeAnimationLength = 0.1f;
    float dodgeTimer;

    bool canDodge = true;
    bool isDodging;


    [Header("Block")]
    bool canBlock = true;
    bool isBlocking;

    [Header("Attack")]
    Fighter fighter;
    CombatController combatController;
    CombatTarget target = null;
    [SerializeField] float attackMovementDuration = 0.05f;
    [SerializeField] float attackMovementSpeed = 20f;
    [SerializeField] float attackMovementToTargetThreshold = 3f;
    bool canAttackMove;
    bool isAttackMoving;
    bool isDashAttacking;

    [Header("Animation")]
    private AnimationScheduler animationScheduler;
    AnimationPlayer playerAnimation;

    bool isRollDodgeJumping = false;
    bool canRollDodgeJump = true;
    bool isRollDodging = false;
    bool isRollDodgeJumpDAttacking = false;
    bool isRollDodgeDAttacking;

    bool isAttackingFlip;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        inputHandler = GetComponent<PlayerInputHandler>();
        combatController = GetComponent<CombatController>();
        groundCheck = groundCheckTransform.GetComponent<GroundCheck>();
        animationScheduler = GetComponent<AnimationScheduler>();
        fighter = GetComponent<Fighter>();
        playerAnimation = GetComponent<AnimationPlayer>();
        lastPosition = transform.position;
    }

    private void Update()
    {
        SetActionBools();
        CameraMove();
        HandleJump();
        GravitySetter();
        HandleMovement();
        HandleSprint();
        HandleDodgeAndRoll();
        HandleLockOn();
        ControlLockOn();
        BlockHandler();
        //HandleAttack();

    }

    private void SetActionBools()
    {
        isRollDodgeJumpDAttacking = isRolling || isDodging || isJumping || isDashAttacking;
        isRollDodgeDAttacking = isRolling || isDodging || isDashAttacking;
        isRollDodgeJumping = isRolling || isDodging || isJumping;
        canRollDodgeJump = canRoll && canDodge && canJump && !isDashAttacking;
        isRollDodging = isRolling || isDodging;
        movementDisabled = !isRollDodgeDAttacking;
    }

    private void CameraMove()
    {
        Vector2 cameraMoveInput = lockingOn ? Vector2.zero : inputHandler.cameraMoveInput;
        pitch -= cameraMoveInput.y * cameraSensitivity;
        pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);
        if (lockingOn) { print("true"); }
        if (!lockedOn)
        {
            lockOnCamera.Priority = 10;
            playerCamera.Priority = 20;
            if (!lockingOn) { yaw += cameraMoveInput.x * cameraSensitivity; }

            cameraPivot.localRotation = Quaternion.Euler(0f, yaw, 0f); // Possibly include in if block above
            cameraTarget.localRotation = Quaternion.Euler(pitch, 0f, 0f); // same

        }
        else
        {
            lockOnCamera.Priority = 20;
            playerCamera.Priority = 10;

            cameraPivot.rotation = transform.rotation;

            SetLockOnCameraPositionAndRotation();

            if (JoystickMovedThisFrame(cameraMoveInput))
            {
                CombatTarget newTarget = combatController.SwitchTarget(target, cameraMoveInput);
                target = newTarget;
            }
        }

        if (lockedOn && !wasLockedOn)
        {
            if (unlockCoroutine != null) { StopCoroutine(unlockCoroutine); }
            lockOnCoroutine = StartCoroutine(CameraLockOnDelay());
            StartCoroutine(LockingOn());
        }
        else if (!lockedOn && wasLockedOn)
        {
            if (lockOnCoroutine != null) { StopCoroutine(lockOnCoroutine); }
            unlockCoroutine = StartCoroutine(CameraUnlockDelay());
            StartCoroutine(LockingOn());
        }

        wasLockedOn = lockedOn;
    }

    private void SetLockOnCameraPositionAndRotation()
    {
        if (target == null) { return; }
        Vector3 playerToTargetVector = target.transform.position - transform.position;
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
        lockingOn = false;
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
        lockingOn = true;

        yield return new WaitForSeconds(4f);

        lockingOn = false;
    }

    private void HandleJump()
    {
        if (inputHandler.jumpPressed)
        {
            if (groundCheck.grounded && canRollDodgeJump && !playerAnimation.isAttacking)
            {
                Jump();
            }
        }
    }

    private void Jump()
    {
        canBlock = false;
        isJumping = true;
        canJump = false;
        animationScheduler.JumpStartAnimation(this.gameObject);
        velocity.y += jumpForce;
    }

    private void HandleEndOfJump()
    {
        animationScheduler.JumpEndAnimation(this.gameObject);
        StartCoroutine(JumpLagTime());
    }

    private IEnumerator JumpLagTime()
    {
        movementDisabled = true;
        yield return new WaitForSeconds(jumpLagTime);
        movementDisabled = false;
        canJump = true;
        isJumping = false;
        canBlock = true;
    }

    private void GravitySetter()
    {
        if (isJumping)
        {
            velocity.y += gravity * Time.deltaTime;
            if (groundCheck.grounded && velocity.y <= -2)
            {
                HandleEndOfJump();
            }
        }
        else if (!groundCheck.grounded)
        {
            velocity.y += gravity * Time.deltaTime;
        }
        else 
        {
            velocity.y = isDodging ? -0.8f : -2f;
        }
    }

    private void HandleMovement()
    {
        if (!isRollDodgeJumpDAttacking)
        {
            movementDisabled = playerAnimation.isAttacking ? true : false;
        }

        if (movementDisabled) { isMoving = false; }
        Move();
    }

    private void Move()
    {
        float speed;
        if (lockedOn) { speed = combatMovementSpeed; }
        else { speed = sprinting ? sprintSpeed : movementSpeed; }

        Vector3 movementDirection = GetMovementDirection();
        velocity.x = movementDirection.x * speed;
        velocity.z = movementDirection.z * speed;
        if (!movementDisabled)
        {
            controller.Move(velocity * Time.deltaTime);
        }
        isMoving = movementDirection.magnitude > 0.01f ? true : false;

        if (lockedOn) { LockedOnMovementOrientation(); }
        else { DefaultMovementOrientation(movementDirection); }
    }

    private Vector3 GetMovementDirection()
    {
        Quaternion orientation = lockedOn ? lockOnCamera.State.FinalOrientation : playerCamera.State.FinalOrientation;

        Vector3 cameraForwardDirection = orientation * Vector3.forward;
        Vector3 cameraRightDirection = orientation * Vector3.right;

        cameraForwardDirection.y = 0f;
        cameraRightDirection.y = 0f;
        cameraForwardDirection.Normalize();
        cameraRightDirection.Normalize();
        Vector2 moveInput = movementDisabled ? Vector2.zero : inputHandler.moveInput;
        Vector3 movementDirection = cameraRightDirection * moveInput.x + cameraForwardDirection * moveInput.y;
        return movementDirection;
    }

    private void LockedOnMovementOrientation()
    {
        if (target != null)
        {
            Vector3 playerToTargetVector = (target.transform.position - transform.position);
            playerToTargetVector.y = 0f;
            playerToTargetVector.Normalize();

            Quaternion targetRotation = Quaternion.LookRotation(playerToTargetVector);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * playerRotationSpeed);
            animationScheduler.CombatMovementAnimation(this.gameObject, inputHandler.moveInput);
        }
    }

    private void DefaultMovementOrientation(Vector3 movementDirection)
    {
        if (movementDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movementDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * playerRotationSpeed);
        }

        animationScheduler.MovementAnimation(this.gameObject);
    }

    private void HandleSprint()
    {
        if (inputHandler.sprintTogglePressed && !isRollDodgeJumping)
        {
            sprinting = !sprinting;
        }
    }

    private void HandleDodgeAndRoll()
    {
        if (inputHandler.dodgePressed)
        {
            if (canRollDodgeJump && !playerAnimation.isAttacking)
            {
                animationScheduler.ResetAttackCycle(this.gameObject);
                if (lockedOn)
                {
                    currentDodgeInput = inputHandler.moveInput;
                    StartDodge(currentDodgeInput);
                    canDodge = false;

                }
                else
                {
                    rollDirection = GetMovementDirection().normalized;
                    StartRoll();
                    canRoll = false;
                }
            }
        }

        if (isRolling) { Roll(rollDirection); }
        if (isDodging) { Dodge(currentDodgeInput); }
    }

    private void StartRoll()
    {
        isRolling = true;
        movementDisabled = true;
        canBlock = false;
        rollTimer = rollAnimationLength;


        animationScheduler.EndAllMovementAnimations(this.gameObject);
        animationScheduler.RollAnimation(this.gameObject);
    }

    private void Roll(Vector3 direction)
    {
        Vector3 rollDirection;

        rollDirection = direction.x == 0 && direction.z == 0 ? transform.forward : direction;
        transform.rotation = Quaternion.LookRotation(rollDirection);

        rollDirection.y = -2f;
        rollDirection.Normalize();

        Vector3 rollMovement = rollDirection * rollSpeed;

        rollTimer -= Time.deltaTime;

        if (rollTimer > rollLagTimeAnimationLength)
        {
            controller.Move(rollMovement * Time.deltaTime);
        }
        else if (rollTimer <= rollLagTimeAnimationLength)
        {
            controller.Move(Vector3.zero);
        }
        if (rollTimer <= 0f)
        {
            StartCoroutine(RollLagTime());
        }
    }

    private IEnumerator RollLagTime()
    {
        isRolling = false;
        movementDisabled = false;
        canBlock = true;
        animationScheduler.EndRollAnimation(this.gameObject);
        yield return new WaitForSeconds(0.32f);
        canRoll = true;
    }

    private void StartDodge(Vector2 input)
    {
        isDodging = true;
        movementDisabled = true;
        canBlock = false;

        dodgeTimer = dodgeAnimationLength;

        animationScheduler.EndAllMovementAnimations(this.gameObject);
        Vector2 HVInput = HardInputCalculator(input);
        animationScheduler.DodgeAnimation(this.gameObject, HVInput);
    }

    private Vector2 HardInputCalculator(Vector2 input)
    {
        float horizontalInput = input.x;
        float verticalInput = input.y;
        float threshold = 0.2f;

        float horizontal = Mathf.Abs(horizontalInput) >= threshold ? Mathf.Sign(horizontalInput) : 0f;
        float vertical = Mathf.Abs(verticalInput) >= threshold ? Mathf.Sign(verticalInput) : 0f;

        return new Vector2(horizontal, vertical);
    }

    private void Dodge(Vector2 input)
    {
        Vector3 dodgeDirection = GetDodgeDirection(input);
        Vector3 dodgeMovement = dodgeDirection * dodgeSpeed;

        dodgeTimer -= Time.deltaTime;

        if (dodgeTimer > dodgeLagTimeAnimationLength)
        {
            controller.Move(dodgeMovement * Time.deltaTime);
        }
        else if (dodgeTimer <= dodgeLagTimeAnimationLength)
        {
            controller.Move(Vector3.zero);
        }
        if (dodgeTimer <= 0f)
        {
            StartCoroutine(DodgeLagTime());
        }

        HandlePlayerOrientationForDodging();
    }

    private Vector3 GetDodgeDirection(Vector2 input)
    {
        Vector2 inputDirection = HardInputCalculator(input);
        Vector3 dodgeDirection = DodgeDirectionCalculator(inputDirection);

        return dodgeDirection;
    }

    private Vector3 DodgeDirectionCalculator(Vector2 inputDirection)
    {
        Vector3 direction = Vector3.zero;

        Vector3 playerForward = transform.forward;
        Vector3 playerRight = Vector3.Cross(Vector3.up, playerForward).normalized;

        playerForward.y = 0f;
        playerRight.y = 0f;

        if (Mathf.Abs(inputDirection.x) == 1f)
        {
            if (Mathf.Abs(inputDirection.y) == 1f) { dodgeSpeed = 8f; }
            else { dodgeSpeed = 10f; }
        }
        else { dodgeSpeed = 4f; }

        if (inputDirection == Vector2.zero)
        {
            direction = -playerForward;
        }
        else if (inputDirection.y == 1)
        {
            if (inputDirection.x == -1)
            {
                direction = (playerForward - playerRight).normalized;
            }
            else if (inputDirection.x == 0)
            {
                direction = playerForward;
            }
            else
            {
                direction = (playerForward + playerRight).normalized;
            }
        }
        else if (inputDirection.y == 0)
        {
            if (inputDirection.x == -1)
            {
                direction = -playerRight;
            }
            else if (inputDirection.x == 1)
            {
                direction = playerRight;
            }
        }
        else
        {
            if (inputDirection.x == -1)
            {
                direction = (-playerForward - playerRight).normalized;
            }
            else if (inputDirection.x == 0)
            {
                direction = -playerForward;
            }
            else
            {
                direction = (-playerForward + playerRight).normalized;
            }
        }

        direction.y = -1f;
        direction.Normalize();
        return direction;
    }

    private void HandlePlayerOrientationForDodging()
    {
        if (target == null) { return; }
        Vector3 vectorToTarget = target.transform.position - transform.position;
        vectorToTarget.y = 0f;
        Quaternion targetRotation = Quaternion.LookRotation(vectorToTarget.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * playerRotationSpeed);
    }

    private IEnumerator DodgeLagTime()
    {
        movementDisabled = false;
        canBlock = true;
        isDodging = false;
        animationScheduler.EndDodgeAnimation(this.gameObject);
        yield return new WaitForSeconds(0.5f);
        canDodge = true;
    }

    private void HandleLockOn()
    {
        if (inputHandler.lockOnTogglePressed)
        {
            LockOn();
        }
    }

    private void LockOn()
    {
        if (target == null)
        {
            target = combatController.LockOn();
        }
        else { target = null; }
    }

    private void ControlLockOn()
    {
        if (target == null)
        {
            lockedOn = false;
            animationScheduler.ExitCombatAnimation(this.gameObject);
        }
        else
        {
            lockedOn = true;
            animationScheduler.EnterCombatAnimation(this.gameObject);
            float distance = DistanceToTarget();
            if (distance > combatController.sightDistance) { target = null; }
        }
    }

    private float DistanceToTarget()
    {
        if (target == null)
        {
            return Mathf.Infinity;
        }
        float distance = (transform.position - target.transform.position).magnitude;
        return distance;
    }

    //private void HandleAttack()
    //{
    //    if (inputHandler.attackPressed && !isRollDodgeJumping)
    //    {
    //        Vector2 horizontalVelocity = new Vector2(velocity.x, velocity.z);
    //        if (horizontalVelocity.magnitude >= 3.9)
    //        {
    //            if (!isDashAttacking)
    //            {
    //                isDashAttacking = true;
    //                fighter.DashAttack();
    //            }
    //        }
    //        else
    //        {
    //            fighter.Attack();
    //        }
    //    }
    //    HandleAttackMovement();
    //}

    private void HandleAttackMovement()
    {
        if (playerAnimation.attackTriggeredThisFrame)
        {
            {
                if (HardInputCalculator(inputHandler.moveInput).y == 1)
                {
                    TriggerAttackMovement();
                }
            }
        }
    }

    private void TriggerAttackMovement()
    {
        if (!isAttackMoving)
        {
            if ((lockedOn && DistanceToTarget() >= attackMovementToTargetThreshold) || !lockedOn)
            {
                StartCoroutine(AttackMovement());
            }
        }
    }

    private IEnumerator AttackMovement()
    {
        isAttackMoving = true;
        float elapsed = 0f;

        while (elapsed < attackMovementDuration)
        {
            float speed = Mathf.Lerp(attackMovementSpeed, 0f, elapsed / attackMovementDuration);

            Vector3 movement = transform.forward * speed * Time.deltaTime;
            controller.Move(movement);

            elapsed += Time.deltaTime;

            yield return null;
        }
        isAttackMoving = false;
    }

    private void BlockHandler()
    {

        if (playerAnimation.attackTriggeredThisFrame)
        {
            canBlock = playerAnimation.isAttacking ? false : true;
        }

        isBlocking = inputHandler.blocking && canBlock ? true : false;

        Block();
    }

    private void Block()
    {
        if (isBlocking)
        {
            animationScheduler.StartBlockAnimation(this.gameObject);
        }
        else
        {
            animationScheduler.EndBlockAnimation(this.gameObject);
        }
    }
}