using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationTests : MonoBehaviour
{
    [SerializeField] private Animator animator;

    public float walkSpeedMulitplier = 0.75f;
    public float idleDuration;
    public float walkDuration;
    public float attackDuration;
    public float abilityDuration;
    public float deathDuration;
    
    
    public void GetAnimatorInfos()
    {
        if (animator == null) return;

        Debug.Log($"Does this have override controller ? : {animator.runtimeAnimatorController is AnimatorOverrideController}");
        if (animator.runtimeAnimatorController is not AnimatorOverrideController overrideController) return;

        idleDuration = overrideController.animationClips[0].length;
        walkDuration = overrideController.animationClips[1].length * 1/walkSpeedMulitplier;
        attackDuration = overrideController.animationClips[2].length;
        abilityDuration = overrideController.animationClips[3].length;
        deathDuration = overrideController.animationClips[4].length;
    }

    public void PlayAttackAnimation()
    {
        if (animator == null) return;
        
        animator.SetTrigger("Attack");
        
        Debug.Log($"{animator.GetNextAnimatorClipInfoCount(0)}");
        
    }
    
    public void PlayAbilityAnimation()
    {
        if (animator == null) return;
        
        animator.SetTrigger("Ability");
    }

    public void PlayWalkingAnimation(bool value)
    {
        if (animator == null) return;
        
        animator.SetBool("IsWalking",value);
    }
    
    public void PlayDeathAnimation(bool value)
    {
        if (animator == null) return;
        
        animator.SetBool("IsDead",value);
    }
    
    public void ToggleDeath()
    {
        if (animator == null) return;
        
        animator.SetBool("IsDead", !animator.GetBool("IsDead"));
    }

    public void ToggleWalk()
    {
        if (animator == null) return;
        
        animator.SetBool("IsWalking", !animator.GetBool("IsWalking"));
    }
}
