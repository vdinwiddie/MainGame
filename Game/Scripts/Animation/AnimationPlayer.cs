using UnityEditorInternal;
using UnityEngine;

public class AnimationPlayer : MonoBehaviour, IAnimation
{
    Animator animator;
    AnimatorStateInfo currentStateInfo;
    int lastStateHash;

    Vector3 lastPosition;
    float timeBetweenAttacks;
    float animationSpeedModifier;
    float timeSinceLastAttack = Mathf.Infinity;
    float attackComboReturnTime = 1.2f;
    float currentClipLength = 0;

    public bool isAttacking = false;
    bool attackTriggerFlippedThisFrame;
    bool wasFlipped;
    public bool attackTriggeredThisFrame;

    private void Start()
    {
        animator = GetComponent<Animator>();
        lastStateHash = -1;
        lastPosition = transform.position;
    }

    private void Update()
    {
        timeSinceLastAttack += Time.deltaTime;
        lastPosition = transform.position;
        bool currentAttacking = animator.GetBool("isAttacking");

        if (animator.GetBool("isAttacking")) { SetClipLength(); }

        isAttacking = currentClipLength > timeSinceLastAttack ? true : false;

        SetAttackTriggeredThisFrame();

        if (!isAttacking)
        {
            EndAttackAnimation();
        }

        if (timeSinceLastAttack > attackComboReturnTime)
        {
            ResetAttackCycle();
        }
    }

    private void SetAttackTriggeredThisFrame()
    {
        if (attackTriggerFlippedThisFrame != wasFlipped)
        {
            attackTriggeredThisFrame = true;
        }
        else
        {
            attackTriggeredThisFrame = false;
        }

        attackTriggerFlippedThisFrame = wasFlipped;
    }

    private void SetClipLength()
    {
        AnimatorClipInfo[] clipInfos = animator.GetCurrentAnimatorClipInfo(0);
        if (clipInfos.Length > 0)
        {
            var currentClip = clipInfos[0].clip;
            string clipName = currentClip.name;

            if (clipName.Contains("***")) { return; }
            float speedModifier = animator.GetCurrentAnimatorStateInfo(0).speed;
            currentClipLength = currentClip.length / (speedModifier * animator.speed);
        }
    }

    public void MovementAnimation()
    {
        Vector3 velocity = (transform.position - lastPosition) / Time.deltaTime;
        animator.SetFloat("forwardSpeed", velocity.magnitude, 0.1f, Time.deltaTime);
    }

    public void JumpStartAnimation()
    {
        animator.ResetTrigger("hitGround");
        animator.SetTrigger("jumpTriggered");
    }

    public void JumpEndAnimation()
    {
        animator.SetTrigger("hitGround");
    }

    public void RollAnimation()
    {
        animator.SetBool("isRolling", true);
    }

    public void EndRollAnimation()
    {
        animator.SetBool("isRolling", false);
    }

    public void CombatMotionAnimation(Vector2 input)
    {
        animator.SetFloat("horizontal", input.x);
        animator.SetFloat("vertical", input.y);
    }

    public void CombatMotionAnimationEnter()
    {
        animator.SetBool("lockedOn", true);
    }

    public void CombatMotionAnimationExit()
    {
        animator.SetBool("lockedOn", false);
    }

    public void DodgeAnimation(Vector2 input)
    {
        animator.SetBool("isDodging", true);
        animator.SetFloat("horizontal", Mathf.Sign(input.x));
        animator.SetFloat("vertical", Mathf.Sign(input.y));
    }

    public void EndDodgeAnimation()
    {
        animator.SetBool("isDodging", false);
    }

    public void StartBlockAnimation()
    {
        animator.SetLayerWeight(1, 1f);
        animator.SetBool("isBlocking", true);
    }

    public void EndBlockAnimation()
    {
        float targetWeight = 0f;
        float currentWeight = animator.GetLayerWeight(1);
        float weightTransition = Mathf.Lerp(currentWeight, targetWeight, 10f * Time.deltaTime);
        animator.SetLayerWeight(1, weightTransition);
        animator.SetBool("isBlocking", false);
    }

    public void SetAttackSpeed(float attackSpeed)
    {
        timeBetweenAttacks = attackSpeed;
    }

    public void SetSpeedModifier(float speedModifier)
    {
        animationSpeedModifier = speedModifier;
    }

    public void AttackAnimation(int attackInCombo)
    {
        timeSinceLastAttack = 0;

        attackTriggerFlippedThisFrame = true;
        animator.speed = animationSpeedModifier;
        animator.SetBool("isAttacking", true);
        animator.SetInteger("attack", attackInCombo);
        animator.SetTrigger("attackTriggered");
    }

    public void EndAttackAnimation()
    {
        if (animator.GetBool("isAttacking"))
        {
            animator.SetBool("isAttacking", false);
        }
    }

    public bool CanAttack()
    {
        if (timeSinceLastAttack >= timeBetweenAttacks)
        {
            return true;
        }
        return false;
    }

    public void ResetAttackTrigger()
    {
        animator.ResetTrigger("attackTriggered");
    }

    public void ResetAttackCycle()
    {
        GetComponent<Fighter>().attackInComboCycle = 0;
    }

    public void DashAttackAnimation()
    {
        animator.SetTrigger("dashAttackTriggered");
    }

    public void EndAllMovementAnimations()
    {
        if (animator.GetBool("isAttacking")) { animator.SetBool("isAttacking", false); }
        if (animator.GetBool("isBlocking")) { animator.SetBool("isBlocking", false); }
    }
}
