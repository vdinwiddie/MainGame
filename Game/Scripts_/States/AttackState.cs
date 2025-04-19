using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "States/Attack")]
public class AttackState : PlayerState, IReceivesAnimationEvent
{
    int currentIndex = 0;
    bool inputQueued = false;
    bool hasTriggeredAttackMoveThisState;
    float timer;
    Weapon currentWeapon;
    Coroutine comboCoroutine;

    public override void OnEnter(PlayerStateMachine player)
    {
        player.context.canJump = false;
        player.context.canRoll = false;
        player.context.canDodge = false;
        player.context.canToggleSprint = false;
        player.context.canMove = false;
        player.animator.applyRootMotion = false;

        hasTriggeredAttackMoveThisState = false;
        currentIndex = 0;
        currentWeapon = player.SetWeapon(player.context.currentWeapon);
        Attack(player);
    }

    public override void OnUpdate(PlayerStateMachine player)
    {
        if (!hasTriggeredAttackMoveThisState && player.HardInputCalculator().y == 1 && !player.isAttackMoving)
        {
            if ((player.context.isLockedOn && player.PlayerToObjectDistance(player.context.target.gameObject) >= player.attackMovementToTargetThreshold) || !player.context.isLockedOn)
            {
                player.StartAttackMovment(player, player.attackMovementDuration, player.attackMovementSpeed);
                hasTriggeredAttackMoveThisState = true;
            }
        }

        if (player.input.dodgePressed && player.context.isLockedOn && player.context.canDodge)
        {
            player.SwitchState(player.dodgeState);
        }

        if (player.input.dodgePressed && !player.context.isLockedOn && player.context.canRoll)
        {
            player.SwitchState(player.rollState);
        }

        if (player.input.jumpPressed && player.context.canJump)
        {
            player.SwitchState(player.jumpState);
        }

        if (player.input.attackPressed && player.context.canAttack)
        {
            player.SwitchState(player.heavyAttackState);
            inputQueued = false;
            currentIndex = 0;
            player.input.ResetAttackFlags();
        }

        if (player.input.attackReleased && player.context.canAttack)
        {
            if (player.input.attackHoldDuration < player.input.attackHoldThreshold)
            {
                inputQueued = true;
            }
            player.input.ResetAttackFlags();
        }
    }

    public override void OnExit(PlayerStateMachine player)
    {

        player.animator.speed = 1f;
        if (comboCoroutine != null) { player.StopCoroutine(comboCoroutine); }
        player.animator.applyRootMotion = true;
    }

    private void Attack(PlayerStateMachine player)
    {
        hasTriggeredAttackMoveThisState = false;

        var comboStep = currentWeapon.comboChain[currentIndex];
        player.animator.Play(comboStep.clip.name);
        player.animator.speed = comboStep.animationSpeed;
        timer = 0f;

        if (comboCoroutine != null) { player.StopCoroutine(comboCoroutine); }
        comboCoroutine = player.StartCoroutine(ComboAttack(player));
    }

    private IEnumerator ComboAttack(PlayerStateMachine player)
    {
        var comboStep = currentWeapon.comboChain[currentIndex];

        while (timer < comboStep.clip.length / comboStep.animationSpeed)
        {
            timer += Time.deltaTime;

            if (inputQueued && timer >= comboStep.inputWindowStart && timer <= comboStep.inputWindowEnd)
            {
                inputQueued = false;
                currentIndex++;

                if (currentIndex < currentWeapon.comboChain.Count)
                {
                    Attack(player);
                    yield break;
                }
            }
            yield return null;
        }

        if (player.context.target != null) { player.SwitchState(player.lockedOnMotionState); }
        else { player.SwitchState(player.motionState); }
    }

    private void TriggerAttackMovement(PlayerStateMachine player)
    {
        if (!player.isAttackMoving)
        {
            if ((player.context.isLockedOn && player.PlayerToObjectDistance(player.context.target.gameObject) >= player.attackMovementToTargetThreshold) || !player.context.isLockedOn)
            {
                player.StartCoroutine(player.AttackMovement(player, player.attackMovementDuration, player.attackMovementSpeed));
            }
        }
    }



    public void OnAnimationEvent(string eventName, PlayerStateMachine player)
    {
        if (eventName == "ResetActionBools")
        {
            player.context.canJump = true;
            player.context.canDodge = true;
            player.context.canRoll = true;
            player.context.canMove = true;
            if (!player.context.isLockedOn) { player.context.canToggleSprint = true; }
        }
    }
}
