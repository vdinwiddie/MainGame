using UnityEngine;

[CreateAssetMenu(menuName = "States/Jump")]
public class JumpState : PlayerState, IReceivesAnimationEvent
{
    public override void OnEnter(PlayerStateMachine player)
    {

        player.velocity.y += player.jumpForce;

        player.animator.CrossFadeInFixedTime("JumpStart", 0.1f);
        player.context.canJump = false;
        player.context.canRoll = false;
        player.context.canDodge = false;
        player.context.canToggleSprint = false;
        player.context.canAttack = false;

    }

    public override void OnUpdate(PlayerStateMachine player)
    {
        if (player.input.attackReleased && player.DistanceToGround() >= 0.5f)
        {
            if (player.input.attackHoldDuration < player.input.attackHoldThreshold)
            {

                player.SwitchState(player.jumpAttackState);
            }
            player.input.ResetAttackFlags();
        }

        float speed;
        if (player.context.isLockedOn) { speed = player.combatMovementSpeed; }
        else { speed = player.context.isSprinting ? player.sprintSpeed : player.movementSpeed; }
        
        Vector3 movementDirection = player.GetMovementDirection();
        Quaternion targetRotation;

        player.controller.Move(player.velocity * Time.deltaTime);

        player.velocity.x = movementDirection.x * speed;
        player.velocity.z = movementDirection.z * speed;
        player.velocity.y += player.gravity * Time.deltaTime;

        if (player.context.target != null)
        {
            Vector3 playerToTarget = player.PlayerToObjectVector(player.context.target.gameObject);
            playerToTarget.y = 0;
            playerToTarget.Normalize();
            targetRotation = Quaternion.LookRotation(playerToTarget);
            player.transform.rotation = Quaternion.Slerp(player.transform.rotation, targetRotation, player.playerRotationSpeed * Time.deltaTime);
        }
        else
        {
            if (movementDirection.x == 0 && movementDirection.z == 0)
            {
                targetRotation = Quaternion.LookRotation(player.transform.forward);
            }
            else
            {
                targetRotation = Quaternion.LookRotation(movementDirection);
            }
            player.transform.rotation = Quaternion.Slerp(player.transform.rotation, targetRotation, player.playerRotationSpeed * Time.deltaTime);
        }



        if (player.context.isGrounded && player.velocity.y <= -2f)
        {
            player.animator.CrossFade("JumpEnd", 0.1f);
            player.velocity = new Vector3(0f, 0f, 0f); //player.velocity.y
        }
    }

    public override void OnExit(PlayerStateMachine player)
    {

    }

    public void OnAnimationEvent(string eventName, PlayerStateMachine player)
    {
        if (eventName == "JumpEnd")
        {
            if (player.context.isLockedOn)
            {
                player.animator.CrossFade("CombatMotion", 0.1f);
                player.SwitchState(player.lockedOnMotionState);
            }

            else
            {
                player.animator.CrossFade("Motion", 0.1f);
                player.SwitchState(player.motionState);
            }

            player.context.canJump = true;
            player.context.canRoll = true;
            player.context.canDodge = true;
            player.context.canToggleSprint = true;
            player.context.canAttack = true;
        }
    }
}
