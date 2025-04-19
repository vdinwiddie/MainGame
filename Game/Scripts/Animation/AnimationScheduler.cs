using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationScheduler : MonoBehaviour
{
    public void MovementAnimation(GameObject _gameObject)
    {
        if (_gameObject.TryGetComponent<IAnimation>(out IAnimation animation))
        {
            animation.MovementAnimation();
        }
    }

    public void JumpStartAnimation(GameObject _gameObject)
    {
        if (_gameObject.TryGetComponent<IAnimation>(out IAnimation animation))
        {
            animation.JumpStartAnimation();
        }
    }

    public void JumpEndAnimation(GameObject _gameObject)
    {
        if (_gameObject.TryGetComponent<IAnimation>(out IAnimation animation))
        {
            animation.JumpEndAnimation();
        }
    }

    public void RollAnimation(GameObject _gameObject)
    {
        if (_gameObject.TryGetComponent<IAnimation>(out IAnimation animation))
        {
            animation.RollAnimation();
        }
    }

    public void EndRollAnimation(GameObject _gameObject)
    {
        if (_gameObject.TryGetComponent<IAnimation>(out IAnimation animation))
        {
            animation.EndRollAnimation();
        }
    }

    public void CombatMovementAnimation(GameObject _gameObject, Vector2 input)
    {
        if (_gameObject.TryGetComponent<IAnimation>(out IAnimation animation))
        {
            animation.CombatMotionAnimation(input);
        }

    }

    public void EnterCombatAnimation(GameObject _gameObject)
    {
        if (_gameObject.TryGetComponent<IAnimation>(out IAnimation animation))
        {
            animation.CombatMotionAnimationEnter();
        }
    }

    public void ExitCombatAnimation(GameObject _gameObject)
    {
        if (_gameObject.TryGetComponent<IAnimation>(out IAnimation animation))
        {
            animation.CombatMotionAnimationExit();
        }
    }

    public void DodgeAnimation(GameObject _gameObject, Vector2 input)
    {
        if (_gameObject.TryGetComponent<IAnimation>(out IAnimation animation))
        {
            animation.DodgeAnimation(input);
        }
    }

    public void EndDodgeAnimation(GameObject _gameObject)
    {
        if (_gameObject.TryGetComponent<IAnimation>(out IAnimation animation))
        {
            animation.EndDodgeAnimation();
        }
    }

    public void StartBlockAnimation(GameObject _gameObject)
    {
        if (_gameObject.TryGetComponent<IAnimation>(out IAnimation animation))
        {
            animation.StartBlockAnimation();
        }
    }

    public void EndBlockAnimation(GameObject _gameObject)
    {
        if (_gameObject.TryGetComponent<IAnimation>(out IAnimation animation))
        {
            animation.EndBlockAnimation();
        }
    }

    public bool CanAttack(GameObject _gameObject)
    {
        if (_gameObject.TryGetComponent<IAnimation>(out IAnimation animation))
        {
            return animation.CanAttack();
        }
        return false;
    }

    public void SetAttackSpeed(GameObject _gameObject, float attackSpeed)
    {
        if (_gameObject.TryGetComponent<IAnimation>(out IAnimation animation))
        {
            animation.SetAttackSpeed(attackSpeed);
        }
    }

    public void SetSpeedModifier(GameObject _gameObject, float modifier)
    {
        if (_gameObject.TryGetComponent<IAnimation>(out IAnimation animation))
        {
            animation.SetSpeedModifier(modifier);
        }
    }

    public void AttackAnimation(GameObject _gameObject, int attackInCombo)
    {
        if (_gameObject.TryGetComponent<IAnimation>(out IAnimation animation))
        {
            animation.AttackAnimation(attackInCombo);
        }
    }

    public void EndAttackAnimation(GameObject _gameObject)
    {
        if (_gameObject.TryGetComponent<IAnimation>(out IAnimation animation))
        {
            animation.EndAttackAnimation();
        }
    }

    public void ResetAttackCycle(GameObject _gameObject)
    {
        if (_gameObject.TryGetComponent<IAnimation>(out IAnimation animation))
        {
            animation.ResetAttackCycle();
        }
    }

    public void EndAllMovementAnimations(GameObject _gameObject)
    {
        if (_gameObject.TryGetComponent<IAnimation>(out IAnimation animation))
        {  animation.EndAllMovementAnimations(); }
    }
}
