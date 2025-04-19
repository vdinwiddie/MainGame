using UnityEngine;

[System.Serializable]
public class PlayerStateContext
{
    [Header("Jumping")]
    public bool canJump = true;
    public float jumpLagTime = 0.1f;

    [Header("Movement")]
    public bool canMove = true;
    public bool canToggleSprint = true;
    public bool isSprinting;

    [Header("Rolling")]
    public bool canRoll = true;
    public float rollCoolDown = 0f;

    [Header("Dodging")]
    public bool canDodge = true;

    [Header("Attacking")]
    public Weapon currentWeapon;
    public float attackCoolDown = 0f;
    public bool canAttack = true;

    [Header("Blocking")]
    public bool canBlock = true;

    [Header("Targeting")]
    public CombatTarget target;
    public float lockOnDistance;
    public bool isLockedOn;
    public bool wasLockedOn;
    public bool isLockingOn;
    public bool canToggleLockOn = true;

    [Header("Physics")]
    public bool isGrounded = true;





    // public float stamina
}
