using System.Collections;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

[CreateAssetMenu(menuName = "States/Dodge")]
public class DodgeState : PlayerState, IReceivesAnimationEvent
{
    public override void OnEnter(PlayerStateMachine player)
    {
        player.context.canJump = false;
        player.context.canMove = false;
        player.context.canRoll = false;
        player.context.canDodge = false;
        player.context.canAttack = false;
        player.context.canBlock = false;
        player.dodgeAttackQueued = false;
        player.animator.applyRootMotion = false;

        player.StartCoroutine(InitializeDodge(player));
    }

    public override void OnUpdate(PlayerStateMachine player)
    {
        if (player.input.attackReleased)
        {
            player.dodgeAttackQueued = true;
        }

        HandleDodgeMovement(player);
    }

    public override void OnExit(PlayerStateMachine player)
    {
        player.animator.applyRootMotion = true;
    }

    private IEnumerator InitializeDodge(PlayerStateMachine player)
    {
        float xInput = player.HardInputCalculator().x;
        float yInput = player.HardInputCalculator().y;

        player.animator.SetFloat("horizontal", xInput);
        player.animator.SetFloat("vertical", yInput);
        player.animator.Play("Dodge");

        yield return null;

        SetDodgeDirection(player);

        player.dodgeDuration = player.GetCurrentClipLength() / 2;
        player.dodgeTimer = player.dodgeDuration;
    }

    private void HandleDodgeMovement(PlayerStateMachine player)
    {

        float dodgeProgress = 1f - (player.dodgeTimer / player.dodgeDuration);
        float speedMultiplier = Mathf.Lerp(1f, 0.3f, dodgeProgress);
        Debug.Log($"Duration: {player.dodgeDuration}, Timer: {player.dodgeTimer}");

        player.controller.Move(player.dodgeDirection * player.dodgeSpeed * speedMultiplier * Time.deltaTime);
        HandleLookOrientation(player);
        if (player.dodgeTimer > 0) { player.dodgeTimer -= Time.deltaTime; }
        else
        {
            player.dodgeTimer = 0;
            player.dodgeDirection = new Vector3(0f, -2f, 0f);
        }
    }

    private void SetDodgeDirection(PlayerStateMachine player)
    {
        Vector2 inputDirection = player.HardInputCalculator();
        Vector3 direction = Vector3.zero;
        Vector3 forward = player.transform.forward;
        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

        forward.y = 0f;
        right.y = 0f;

        if (inputDirection == Vector2.zero)
        {
            direction = -forward;
        }
        else if (inputDirection.y == 1)
        {
            if (inputDirection.x == -1)
            {
                direction = (forward - right).normalized;
            }
            else if (inputDirection.x == 0)
            {
                direction = forward;
            }
            else
            {
                direction = (forward + right).normalized;
            }
        }
        else if (inputDirection.y == 0)
        {
            if (inputDirection.x == -1)
            {
                direction = -right;
            }
            else if (inputDirection.x == 1)
            {
                direction = right;
            }
        }
        else
        {
            if (inputDirection.x == -1)
            {
                direction = (-forward - right).normalized;
            }
            else if (inputDirection.x == 0)
            {
                direction = -forward;
            }
            else
            {
                direction = (-forward + right).normalized;
            }
        }

        direction.y = -1f;
        direction.Normalize();

        player.dodgeDirection = direction;
    }

    private void HandleLookOrientation(PlayerStateMachine player)
    {
        if (player.context.target == null) { return; }
        Vector3 vectorToTarget = player.PlayerToObjectVector(player.context.target.gameObject);
        vectorToTarget.y = 0f;
        Quaternion targetRotation = Quaternion.LookRotation(vectorToTarget.normalized);
        player.transform.rotation = Quaternion.Slerp(player.transform.rotation, targetRotation, player.playerRotationSpeed * Time.deltaTime);
    }

    public void OnAnimationEvent(string eventName, PlayerStateMachine player)
    {
        if (eventName == "DodgeAttackEvent" && player.dodgeAttackQueued)
        {
            player.SwitchState(player.dodgeAttackState);
            player.input.ResetAttackFlags();
        }

        if (eventName == "DodgeEnd")
        {
            player.animator.CrossFade("CombatMotion", 0.1f);
            player.SwitchState(player.lockedOnMotionState);

            player.context.canMove = true;
            player.context.canAttack = true;
            player.context.canBlock = true;

            player.StartCoroutine(PostDodgeDelay(player));
        }
        player.dodgeAttackQueued = false;
    }

    private IEnumerator PostDodgeDelay(PlayerStateMachine player)
    {
        yield return new WaitForSeconds(0.2f);
        player.context.canJump = true;
        player.context.canRoll = true;
        player.context.canDodge = true;
    }
}
