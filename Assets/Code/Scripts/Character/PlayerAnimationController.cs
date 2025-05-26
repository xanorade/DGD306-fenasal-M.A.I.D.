using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DGD306.Character
{
    public class PlayerAnimationController : MonoBehaviour
    {
        [Header("Animation References")]
        [SerializeField] private RuntimeAnimatorController animatorController;
        
        private Animator animator;
        private FighterController fighterController;
        
        private void Awake()
        {
            animator = GetComponent<Animator>();
            fighterController = GetComponent<FighterController>();
            
            // Apply animator controller if assigned
            if (animatorController != null && animator != null)
            {
                animator.runtimeAnimatorController = animatorController;
            }
        }
        
        // Method to play a specific animation by name
        public void PlayAnimation(string animationName)
        {
            if (animator != null)
            {
                animator.Play(animationName);
            }
        }
        
        // Method to check if a specific animation is currently playing
        public bool IsPlayingAnimation(string animationName)
        {
            if (animator != null)
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                return stateInfo.IsName(animationName);
            }
            return false;
        }
    }
} 