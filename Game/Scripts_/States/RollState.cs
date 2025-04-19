using UnityEngine;

[CreateAssetMenu(menuName = "States/Roll")]
public class RollState : PlayerState, IReceivesAnimationEvent
{
    public override void OnEnter(PlayerStateMachine player)
    {
        player.context.canJump = false;
        player.context.canMove = false;
        player.context.canToggleSprint = false;
        player.context.canRoll = false;
        player.context.canDodge = false;
        player.context.canAttack = false;
        player.context.canBlock = false;
        player.dodgeAttackQueued = false;

        SetRollDirection(player);
        player.animator.Play("Roll");
        player.rollTimer = player.rollDuration;
    }

    public override void OnUpdate(PlayerStateMachine player)
    {
        if (player.input.attackReleased)
        {
            player.dodgeAttackQueued = true;
        }

        player.transform.rotation = Quaternion.LookRotation(player.rollLookDirection);
        player.controller.Move(player.rollDirection * player.rollSpeed * Time.deltaTime);
        if (player.rollTimer > 0) { player.rollTimer -= Time.deltaTime; }
        else 
        {
            player.rollTimer = 0;
            player.rollDirection = new Vector3(0f, -2f, 0f);
        }
    }

    public override void OnExit(PlayerStateMachine player)
    {
    }

    private static void SetRollDirection(PlayerStateMachine player)
    {
        player.rollDirection = player.GetMovementDirection().normalized;
        if (player.rollDirection.x == 0 && player.rollDirection.z == 0)
        {
            player.rollDirection = player.transform.forward;
        }
        player.rollLookDirection = player.rollDirection;
        player.rollDirection.y = -2f;
        player.rollDirection.Normalize();
    }

    public void OnAnimationEvent(string eventName, PlayerStateMachine player)
    {
        if (eventName == "DodgeAttackEvent" && player.dodgeAttackQueued)
        {
            player.SwitchState(player.dodgeAttackState);
            player.input.ResetAttackFlags();
        }

        else if (eventName == "RollEnd")
        {
            player.animator.CrossFade("Motion", 0.1f);

            player.SwitchState(player.motionState);

            player.context.canJump = true;
            player.context.canMove = true;
            player.context.canToggleSprint = true;
            player.context.canRoll = true;
            player.context.canDodge = true;
            player.context.canAttack = true;
            player.context.canBlock = true;
        }
            player.dodgeAttackQueued = false;
    }
}
