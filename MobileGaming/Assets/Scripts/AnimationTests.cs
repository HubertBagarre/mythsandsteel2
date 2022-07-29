using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AnimationTests : MonoBehaviour
{
    [SerializeField] private Transform unit;
    [SerializeField] private Animator animator;

    public float walkSpeedMultiplier = 0.75f;
    public float walkSpeedExtra = 14f / 10f;
    public float idleDuration;
    public float walkDuration;
    public float attackDuration;
    public float abilityDuration;
    public float deathDuration;

    public void Start()
    {
        var animationClips = (animator.runtimeAnimatorController as AnimatorOverrideController)?.animationClips;
        if (animationClips != null)
        {
            foreach (var VARIABLE in animationClips)
            {
                Debug.Log(VARIABLE.name);
            }

            if (animator != null)
            {
                if (animator.runtimeAnimatorController is AnimatorOverrideController overrideController)
                {
                    idleDuration = overrideController.animationClips[0].length;
                    walkDuration = overrideController.animationClips[1].length;
                    attackDuration = overrideController.animationClips[2].length;
                    abilityDuration = overrideController.animationClips[3].length;
                    deathDuration = overrideController.animationClips[4].length;
                }
            }
        }
    }

    public void MoveForward()
    {
        StartCoroutine(MoveRoutine());
    }

    private IEnumerator MoveRoutine()
    {
        var currentPos = unit.position;
        var targetPos = currentPos + new Vector3(2,0,0);
        unit.DOMove(targetPos,walkDuration*walkSpeedExtra*walkSpeedMultiplier);
        PlayWalkingAnimation(true);
        yield return new WaitForSeconds(walkDuration);
        PlayWalkingAnimation(false);
    }

    public void TurnRight()
    {
        unit.transform.localPosition = new Vector3(-20, 0, 6.92f);
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
