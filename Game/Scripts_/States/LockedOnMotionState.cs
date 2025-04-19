using UnityEngine;

[CreateAssetMenu(menuName = "States/Locked On Motion")]
public class LockedOnMotionState : PlayerState
{
    public override void OnEnter(PlayerStateMachine player)
    {
        player.context.isSprinting = false;
        player.animator.Play("CombatMotion");
    }

    public override void OnUpdate(PlayerStateMachine player)
    {
        Move(player);

        if (player.context.target == null)
        {
            player.SwitchState(player.motionState);
        }

        if (player.input.jumpPressed && player.context.canJump)
        {
            player.SwitchState(player.jumpState);
        }

        if (player.input.dodgePressed && player.context.canDodge)
        {
            player.SwitchState(player.dodgeState);
        }

        if (player.input.attackPressed && player.context.canAttack)
        {
            player.SwitchState(player.heavyAttackState);
            player.input.ResetAttackFlags();
        }

        if (player.input.attackReleased && player.context.canAttack)
        {
            if (player.input.attackHoldDuration < player.input.attackHoldThreshold)
            {
                player.SwitchState(player.attackState);
            }
            player.input.ResetAttackFlags();
        }
    }

    public override void OnExit(PlayerStateMachine player)
    {
        player.animator.SetFloat("horizontal", 0f);
        player.animator.SetFloat("vertical", 0f);
    }

    private void Move(PlayerStateMachine player)
    {
        Vector3 movementDirection = player.GetMovementDirection();
        player.velocity.x = movementDirection.x * player.combatMovementSpeed;
        player.velocity.z = movementDirection.z * player.combatMovementSpeed;

        if (player.context.canMove) { player.controller.Move(player.velocity * Time.deltaTime); }
        MovementOrientation(player);
        Vector2 input = player.input.moveInput;
        player.animator.SetFloat("horizontal", input.x, 0.1f, Time.deltaTime);
        player.animator.SetFloat("vertical", input.y, 0.1f, Time.deltaTime);
    }

    private void MovementOrientation(PlayerStateMachine player)
    {
        if (player.context.target == null) { return; }

        Vector3 playerToTarget = player.PlayerToObjectVector(player.context.target.gameObject);
        playerToTarget.y = 0;
        playerToTarget.Normalize();

        Quaternion targetRotation = Quaternion.LookRotation(playerToTarget);
        player.transform.rotation = Quaternion.Slerp(player.transform.rotation, targetRotation, player.playerRotationSpeed * Time.deltaTime);
    }
}
