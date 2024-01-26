using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] Animator animator = null;


    private int animatorGroundedBool;
    private int animatorRunningSpeed;
    private int animatorRunningBool;
    private int animatorJumpTrigger;
    private int animatorWallSliding;
    
    private int animatorTriggerCrounching;
    
    private int animatorcanClimb;
    private int animatorTriggeridel;
    
    private int animatorWallJump;
    private int animatorClimbing;
    
    private int animatorLanding;
    private int animatorFallTrigger;
    
    public CharacterController characterController;

    private float animatorSpeed;
    
    private void Awake() 
    {
        
            animatorGroundedBool = Animator.StringToHash("Grounded");
            animatorRunningSpeed = Animator.StringToHash("RunningSpeed");
            animatorJumpTrigger = Animator.StringToHash("Jump");
            animatorTriggeridel = Animator.StringToHash("Idel");
            animatorLanding = Animator.StringToHash("Landing");
            animatorFallTrigger = Animator.StringToHash("Fall");
            animatorRunningBool = Animator.StringToHash("IsRun");
            
            
            
            animatorSpeed = this.animator.speed; 
    }
        public void PauseAnimator()
        {
            this.animator.speed = 0;
        }
        public void ResumeAnimator()
        {
            this.animator.speed = animatorSpeed;
        }
        public void PlayIdel_Animation()
        {
            animator.Play("Idle");
        }
        public void UpdateGrounded(bool value)
        {
            animator.SetBool(animatorGroundedBool, value);
        }
        public void UpdateLanding(int value)
        {
            animator.SetInteger(animatorLanding, value);
        }
        public void SetRunningSpeed(int value)
        {
            animator.SetInteger(animatorRunningSpeed, value);
        }

        public void SetRunBool(bool value)
        {
            animator.SetBool(animatorRunningBool, value);
        }
        
        public void SetIdelTrigger()
        {
            animator.SetTrigger(animatorTriggeridel);
        }
        public void SetJumpTrigger()
        {
            animator.SetTrigger(animatorJumpTrigger);
        }
        public void SetWallJumpTrigger()
        {
            animator.SetTrigger(animatorWallJump);
        }
        public void SetFallTrigger()
        {
            animator.SetTrigger(animatorFallTrigger);
        }
}
