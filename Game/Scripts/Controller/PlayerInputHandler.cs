using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerInputActions controls;

    public Vector2 moveInput { get; private set; }
    public Vector2 cameraMoveInput { get; private set; }
    public bool blocking => controls.Player.Block.ReadValue<float>() > 0;
    public bool jumpPressed => controls.Player.Jump.WasPressedThisFrame();
    public bool dodgePressed => controls.Player.Dodge.WasPressedThisFrame();
    public bool sprintTogglePressed => controls.Player.SprintToggle.WasPressedThisFrame();
    public bool lockOnTogglePressed => controls.Player.LockOn.WasPressedThisFrame();
    public bool attackTriggered => controls.Player.Attack.WasPressedThisFrame();
    public bool isHoldingAttack;
    public bool attackPressed { get; private set; }
    public bool attackReleased { get; private set; }
    public float attackHoldDuration { get; private set; }

    private float attackStartTime;
    public float attackHoldThreshold = .1f;
    
    private void Awake()
    {
        controls = new PlayerInputActions();

        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        controls.Player.CameraMove.performed += ctx => cameraMoveInput = ctx.ReadValue<Vector2>();
        controls.Player.CameraMove.canceled += ctx => cameraMoveInput = Vector2.zero;

        controls.Player.Attack.started += ctx =>
        {
            isHoldingAttack = true;
            attackStartTime = Time.time;

        };

        controls.Player.Attack.canceled += ctx =>
        {
            isHoldingAttack = false;
            attackHoldDuration = 0f;
        };

        controls.Player.Attack.performed += OnAttackStarted;
        controls.Player.Attack.canceled += OnAttackCanceled;
    }

    private void Update()
    {
        if (isHoldingAttack) { attackHoldDuration = Time.time - attackStartTime; }
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void OnAttackStarted(InputAction.CallbackContext context)
    {
        attackPressed = true;
        attackReleased = false;
        attackStartTime = Time.time;
    }

    private void OnAttackCanceled(InputAction.CallbackContext context)
    {
        attackReleased = true;
        attackPressed = false;
        attackHoldDuration = 0;
    }

    public void ResetAttackFlags()
    {
        attackPressed = false;
        attackReleased = false;
        attackHoldDuration = 0f;
    }
}
