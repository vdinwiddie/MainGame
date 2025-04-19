using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Weapons/Make New Weapon", order = 0)]
public class Weapon : ScriptableObject
{
    [SerializeField] AnimatorOverrideController animatorOverride = null;
    [SerializeField] GameObject equippedPrefab = null;
    [SerializeField] bool isRightHanded = true;
    [SerializeField] public List<ComboAttackData> comboChain;
    [SerializeField] public AnimationClip[] jumpAttackAnimations;
    [SerializeField] public AnimationClip[] heavyAttackAnimations;
    [SerializeField] public float jumpAttackDistanceToTargetThreshold;


    const string weaponName = "Weapon";

    public float GetAttackComboSpeed(int attackNumber)
    {
        //return timeBetweenAttacks[attackComboNumber];
        //return comboChain[attackNumber].attackTime;
        return 0;
    }

    public int AttacksInCombo()
    {
        //return timeBetweenAttacks.Length;
        return comboChain.Count;
    }

    public float AnimationSpeedModifier(int attackNumber)
    {
        //return comboChain[attackNumber].speedModifier;
        return 0;
    }

    public void Spawn(Transform rightHand, Transform leftHand)
    {
        //DestroyPreviousWeapon(rightHand, leftHand);

        if (equippedPrefab != null)
        {
            Transform hand = GetHandTransform(rightHand, leftHand);
            GameObject weapon = Instantiate(equippedPrefab, hand);
            weapon.name = weaponName;
        }

        //var overrideController = animator.runtimeAnimatorController as AnimatorOverrideController;

        //if (animatorOverride != null)
        //{
        //    animator.runtimeAnimatorController = animatorOverride;
        //}
        //else if (overrideController != null)
        //{
        //    animator.runtimeAnimatorController = overrideController.runtimeAnimatorController;
        //}
    }

    private void DestroyPreviousWeapon(Transform rightHand, Transform leftHand)
    {
        Transform oldWeapon = rightHand.Find(weaponName);
        if (oldWeapon == null) { oldWeapon = leftHand.Find(weaponName); }
        if (oldWeapon == null) { return; }

        oldWeapon.name = "Destroying";
        Destroy(oldWeapon.gameObject);
    }

    private Transform GetHandTransform(Transform rightHand, Transform leftHand)
    {
        Transform hand;
        hand = isRightHanded ? rightHand : leftHand;

        return hand;
    }

    [System.Serializable]
    public class ComboAttackData
    {
        public AnimationClip clip;
        public float animationSpeed = 1;
        public float inputWindowStart;
        public float inputWindowEnd;
    }


}
