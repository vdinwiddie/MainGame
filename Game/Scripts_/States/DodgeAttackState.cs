using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "States/Dodge Attack")]
public class DodgeAttackState : PlayerState, IReceivesAnimationEvent
{
    public override void OnEnter(PlayerStateMachine player)
    {
        player.context.canJump = false;
        player.context.canMove = false;
        player.context.canRoll = false;
        player.context.canDodge = false;
        player.context.canAttack = false;
        player.context.canBlock = false;

        player.animator.CrossFade("DodgeAttack", 0.05f);

        if (!player.isAttackMoving)
        {
            if ((player.context.isLockedOn && player.PlayerToObjectDistance(player.context.target.gameObject) >= player.attackMovementToTargetThreshold) || !player.context.isLockedOn)
            {
                float speed = player.context.isLockedOn ? player.dodgeAttackMovementSpeed : player.rollAttackMovementSpeed;
                player.StartAttackMovment(player, player.dodgeAttackMovementDuration, speed);
            }
        }
    }

    public override void OnUpdate(PlayerStateMachine player)
    {

    }

    public override void OnExit(PlayerStateMachine player)
    {

    }

    private void TriggerAttackMovement(PlayerStateMachine player)
    {
        if (!player.isAttackMoving)
        {
            if ((player.context.isLockedOn && player.PlayerToObjectDistance(player.context.target.gameObject) >= player.attackMovementToTargetThreshold) || !player.context.isLockedOn)
            {
                float speed = player.context.isLockedOn ? player.dodgeAttackMovementSpeed : player.rollAttackMovementSpeed;
                //player.StartCoroutine(player.AttackMovement(player, player.dodgeAttackMovementDuration, speed));
                player.StartAttackMovment(player, player.dodgeAttackMovementDuration, speed);
            }
        }
    }

    public void OnAnimationEvent(string eventName, PlayerStateMachine player)
    {
        if (eventName == "DodgeAttackEnd")
        {
            if (player.context.isLockedOn) { player.SwitchState(player.lockedOnMotionState); }
            else { player.SwitchState(player.motionState); }

            player.context.canJump = true;
            player.context.canMove = true;
            player.context.canRoll = true;
            player.context.canDodge = true;
            player.context.canAttack = true;
            player.context.canBlock = true;
        }
    }
}
