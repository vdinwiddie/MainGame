using UnityEngine;

[CreateAssetMenu(menuName = "States/Heavy Attack")]
public class HeavyAttackState : PlayerState, IReceivesAnimationEvent
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
        player.animator.applyRootMotion = false;


        currentWeapon = player.SetWeapon(player.context.currentWeapon);
        player.animator.CrossFade(currentWeapon.heavyAttackAnimations[0].name, 0.05f);
    }

    public override void OnUpdate(PlayerStateMachine player)
    {
        if (player.input.attackReleased)
        {
            player.input.ResetAttackFlags();
            player.animator.Play(currentWeapon.heavyAttackAnimations[1].name);
        }
    }

    public override void OnExit(PlayerStateMachine player)
    {

    }

    public void OnAnimationEvent(string eventName, PlayerStateMachine player)
    {
        if (eventName == "HeavyAttackEnd")
        {
            if (player.context.isLockedOn) { player.SwitchState(player.lockedOnMotionState); }
            else { player.SwitchState(player.motionState); }

            player.context.canJump = true;
            player.context.canMove = true;
            player.context.canRoll = true;
            player.context.canDodge = true;
            player.context.canAttack = true;
            player.context.canBlock = true;
            player.context.canToggleLockOn = true;
            player.animator.applyRootMotion = true;
        }
    }
}
