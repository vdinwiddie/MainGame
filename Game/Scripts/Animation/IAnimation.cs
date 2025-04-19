using UnityEngine;

public interface IAnimation
{
    void MovementAnimation();
    void JumpStartAnimation();
    void JumpEndAnimation();
    void RollAnimation();
    void EndRollAnimation();
    void CombatMotionAnimation(Vector2 input);
    void CombatMotionAnimationEnter();
    void CombatMotionAnimationExit();
    void DodgeAnimation(Vector2 input);
    void EndDodgeAnimation();
    void StartBlockAnimation();
    void EndBlockAnimation();
    bool CanAttack();
    void SetAttackSpeed(float attackSpeed);
    void SetSpeedModifier(float modifier);
    void AttackAnimation(int attackInCombo);
    void EndAttackAnimation();
    void ResetAttackCycle();
    void EndAllMovementAnimations();
}