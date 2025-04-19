using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class Fighter : MonoBehaviour
{
    [SerializeField] Weapon defaultWeapon = null;
    [SerializeField] Transform rightHand = null;
    [SerializeField] Transform leftHand = null;

    public bool canAttack = true;
    public int attackInComboCycle = 0;
    public Weapon currentWeapon = null;
    AnimationScheduler animationScheduler;


    private void Start()
    {
        EquipWeapon(defaultWeapon);
    }

    private void Update()
    {
    }


    private void EquipWeapon(Weapon weapon)
    {
        currentWeapon = weapon;

        weapon.Spawn(rightHand, leftHand);
    }

    public void Attack()
    {
        if (TryGetComponent<AnimationPlayer>(out AnimationPlayer animation))
        {
            if (animation.CanAttack())
            {
                float attackSpeed = currentWeapon.GetAttackComboSpeed(attackInComboCycle);
                float speedModifier = currentWeapon.AnimationSpeedModifier(attackInComboCycle);
                animationScheduler.SetAttackSpeed(this.gameObject, attackSpeed);
                animationScheduler.SetSpeedModifier(this.gameObject, speedModifier);
                animationScheduler.AttackAnimation(this.gameObject, attackInComboCycle);
            }
        }
    }

    public void DashAttack()
    {
        if (TryGetComponent<AnimationPlayer>(out AnimationPlayer animation))
        {
            animation.DashAttackAnimation();
        }
    }

    //Animation event
    public void ResetAttackTrigger()
    {
        attackInComboCycle++;

        if (attackInComboCycle > currentWeapon.AttacksInCombo() - 1)
        {
            attackInComboCycle = 0;
        }
    }

}
