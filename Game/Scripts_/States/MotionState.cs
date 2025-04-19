using UnityEngine;

[CreateAssetMenu(menuName = "States/Motion")]
public class MotionState : PlayerState
{
    public override void OnEnter(PlayerStateMachine player)
    {
        player.animator.CrossFadeInFixedTime("Motion", 0.25f);
    }

    public override void OnUpdate(PlayerStateMachine player)
    {
        Move(player);
        if (player.input.moveInput == Vector2.zero) { player.context.isSprinting = false; }

        if (player.input.jumpPressed && player.context.canJump)
        {
            player.SwitchState(player.jumpState);
        }
        
        if (player.input.dodgePressed)
        {
            if (!player.context.isLockedOn && player.context.canRoll)
            {
                player.SwitchState(player.rollState);
            }
        }

        if (player.context.target != null)
        {
            player.SwitchState(player.lockedOnMotionState);
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
        player.animator.SetFloat("forwardSpeed", 0f);
    }

    private void Move(PlayerStateMachine player)
    {
        float speed = player.context.isSprinting ? player.sprintSpeed : player.movementSpeed;

        Vector3 movementDirection = player.GetMovementDirection();
        player.velocity.x = movementDirection.x * speed;
        player.velocity.z = movementDirection.z * speed;
        if (player.context.canMove) { player.controller.Move(player.velocity * Time.deltaTime); }

        MovementOrientation(player, movementDirection);
        float forwardSpeed = new Vector3(player.velocity.x, 0, player.velocity.z).magnitude;
        player.animator.SetFloat("forwardSpeed", forwardSpeed, 0.1f, Time.deltaTime);

    }

    private void MovementOrientation(PlayerStateMachine player, Vector3 direction)
    {
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            player.transform.rotation = Quaternion.Slerp(player.transform.rotation, targetRotation, player.playerRotationSpeed * Time.deltaTime);
        }
    }
}
