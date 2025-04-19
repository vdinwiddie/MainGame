using Unity.VisualScripting.FullSerializer;
using UnityEngine;

[CreateAssetMenu(menuName = "States/Jump Attack")]
public class JumpAttackState : PlayerState, IReceivesAnimationEvent
{
    Weapon currentWeapon;

    public override void OnEnter(PlayerStateMachine player)
    {
        player.context.canJump = false;
        player.context.canMove = false;
        player.context.canRoll = false;
        player.context.canDodge = false;
        player.context.canAttack = false;
        player.context.canBlock = false;
        player.context.canToggleLockOn = false;
        player.context.canToggleSprint = false;
        player.animator.applyRootMotion = false;
        player.context.isSprinting = false;


        player.velocity.y = player.jumpAttackLift;

        currentWeapon = player.SetWeapon(player.context.currentWeapon);
        player.animator.CrossFade(currentWeapon.jumpAttackAnimations[0].name, 0.05f);
    }

    public override void OnUpdate(PlayerStateMachine player)
    {
        if (!player.context.isGrounded)
        {
            if (player.context.target == null)
            {
                player.velocity.x = player.transform.forward.x;
                player.velocity.z = player.transform.forward.z;
            }
            else
            {
                Vector3 vectorToTarget = player.PlayerToObjectVector(player.context.target.gameObject);
                vectorToTarget.y = 0;
                float distanceToTarget = vectorToTarget.magnitude;
                vectorToTarget.Normalize();

                if (distanceToTarget < currentWeapon.jumpAttackDistanceToTargetThreshold)
                {
                    player.velocity.x = 0f;
                    player.velocity.z = 0f;
                }
                else
                {
                    player.velocity.x = vectorToTarget.x;
                    player.velocity.z = vectorToTarget.z;
                }
            }
            player.velocity.y += player.gravity * Time.deltaTime;
            player.controller.Move(player.velocity * player.jumpAttackSpeedModifier * Time.deltaTime);
        }
        else if (player.DistanceToGround() < player.groundDistanceAttackTrigger)
        {
            player.animator.speed = 2f;
            player.animator.Play(currentWeapon.jumpAttackAnimations[1].name);
        }
    }

    public override void OnExit(PlayerStateMachine player)
    {

    }


    public void OnAnimationEvent(string eventName, PlayerStateMachine player)
    {
        if (eventName == "JumpAttackEnd")
        {
            player.animator.speed = 1f;
            if (player.context.isLockedOn) { player.SwitchState(player.lockedOnMotionState); }
            else { player.SwitchState(player.motionState); }

            player.context.canJump = true;
            player.context.canMove = true;
            player.context.canRoll = true;
            player.context.canDodge = true;
            player.context.canAttack = true;
            player.context.canBlock = true;
            player.context.canToggleLockOn = true;
            player.context.canToggleSprint = true;
            player.animator.applyRootMotion = true;
        }
    }
}
