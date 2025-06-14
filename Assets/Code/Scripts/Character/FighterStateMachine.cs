using System;
using UnityEngine;

public class FighterStateMachine
{
    private FighterController fighter;
    private FighterState currentState;
    
    public Type CurrentStateType => currentState?.GetType();
    
    public FighterStateMachine(FighterController controller)
    {
        fighter = controller;
        currentState = new IdleState();
        currentState.Enter(fighter, this);
    }
    
    public void ChangeState(FighterState newState)
    {
        // Exit the current state
        currentState.Exit(fighter);
        
        // Enter the new state
        currentState = newState;
        currentState.Enter(fighter, this);
        
        // Trigger the animation for the new state
        fighter.TriggerAnimationByState(newState.GetType());
        
        // Update the state text
        fighter.UpdateStateText(currentState.GetType().Name.Replace("State", ""));
    }
    
    public void UpdateState()
    {
        if (currentState != null)
            currentState.Execute(fighter, this);
    }
    
    public bool CanMove()
    {
        return currentState.CanMove();
    }
}

public abstract class FighterState
{
    // Default duration for attacks
    protected const float STARTUP_FRAMES = 3f / 60f; // 3 frames at 60 FPS
    protected const float ACTIVE_FRAMES = 10f / 60f;  // Increased from 3 to 10 frames for better visibility
    protected const float RECOVERY_FRAMES = 15f / 60f; // Increased from 10 to 15 frames
    
    protected float stateTime;
    
    public virtual void Enter(FighterController fighter, FighterStateMachine stateMachine)
    {
        stateTime = 0f;
    }
    
    public virtual void Execute(FighterController fighter, FighterStateMachine stateMachine)
    {
        stateTime += Time.deltaTime;
    }
    
    public virtual void Exit(FighterController fighter)
    {
        // Base exit - no hitbox logic
    }
    
    public virtual bool CanMove()
    {
        return true;
    }
    
    protected bool IsAnimationFinished(float totalDuration)
    {
        return stateTime >= totalDuration;
    }
}

// Basic character states
public class IdleState : FighterState
{
    public override void Enter(FighterController fighter, FighterStateMachine stateMachine)
    {
        base.Enter(fighter, stateMachine);
    }
}

public class WalkState : FighterState
{
}

// Base class for all jump-related states
public abstract class JumpStateBase : FighterState
{
    protected float jumpDuration = 0.5f;
    
    public override bool CanMove()
    {
        return false;
    }
}

public class JumpStartState : JumpStateBase
{
    private const float START_DURATION = 0.1f; // Duration of the initial jump frame
    
    public override void Enter(FighterController fighter, FighterStateMachine stateMachine)
    {
        base.Enter(fighter, stateMachine);
    }
    
    public override void Execute(FighterController fighter, FighterStateMachine stateMachine)
    {
        base.Execute(fighter, stateMachine);
        
        // Transition to loop state after the start frame
        if (IsAnimationFinished(START_DURATION))
        {
            stateMachine.ChangeState(new JumpLoopState());
        }
    }
}

public class JumpLoopState : JumpStateBase
{
    public override void Enter(FighterController fighter, FighterStateMachine stateMachine)
    {
        base.Enter(fighter, stateMachine);
    }
    
    public override void Execute(FighterController fighter, FighterStateMachine stateMachine)
    {
        base.Execute(fighter, stateMachine);
        
        // Check for mid-air attacks
        // Note: The actual attack state transitions are handled by FighterController
        // when the player presses attack buttons during a jump
        
        // Only transition to idle when actually grounded and some time has passed
        if (fighter.IsGrounded && stateTime > 0.1f) // Small delay to prevent immediate transitions
        {
            stateMachine.ChangeState(new IdleState());
        }
    }
}

public class CrouchState : FighterState
{
    public override void Enter(FighterController fighter, FighterStateMachine stateMachine)
    {
        base.Enter(fighter, stateMachine);
    }
    
    public override void Execute(FighterController fighter, FighterStateMachine stateMachine)
    {
        base.Execute(fighter, stateMachine);
        
        // If the player isn't crouching anymore, return to idle
        if (!fighter.IsCrouching && IsAnimationFinished(0.1f))
        {
            stateMachine.ChangeState(new IdleState());
        }
    }
    
    public override void Exit(FighterController fighter)
    {
        base.Exit(fighter);
    }
}

public class BlockState : FighterState
{
    public override bool CanMove()
    {
        return false;
    }
}

public class DashState : FighterState
{
    private bool isAirDash = false;
    
    public override void Enter(FighterController fighter, FighterStateMachine stateMachine)
    {
        base.Enter(fighter, stateMachine);
        
        // Check if this is an air dash
        Rigidbody2D rb = fighter.GetComponent<Rigidbody2D>();
        if (rb != null && !fighter.IsCrouching)
        {
            isAirDash = !Physics2D.Raycast(fighter.transform.position, Vector2.down, 0.1f);
        }
    }
    
    public override void Execute(FighterController fighter, FighterStateMachine stateMachine)
    {
        base.Execute(fighter, stateMachine);
        
        // State is complete automatically after duration
        // DashCoroutine handles the state transition
    }
    
    public override bool CanMove()
    {
        return false;
    }
}

// Attack states
public class PunchState : FighterState
{
    public override void Enter(FighterController fighter, FighterStateMachine stateMachine)
    {
        base.Enter(fighter, stateMachine);
    }
    
    public override void Execute(FighterController fighter, FighterStateMachine stateMachine)
    {
        base.Execute(fighter, stateMachine);
        
        // State is complete
        if (IsAnimationFinished(STARTUP_FRAMES + ACTIVE_FRAMES + RECOVERY_FRAMES))
        {
            stateMachine.ChangeState(new IdleState());
        }
    }
    
    public override bool CanMove()
    {
        return false;
    }
}

public class KickState : FighterState
{
    public override void Enter(FighterController fighter, FighterStateMachine stateMachine)
    {
        base.Enter(fighter, stateMachine);
    }
    
    public override void Execute(FighterController fighter, FighterStateMachine stateMachine)
    {
        base.Execute(fighter, stateMachine);
        
        // State is complete
        if (IsAnimationFinished(STARTUP_FRAMES + ACTIVE_FRAMES + RECOVERY_FRAMES))
        {
            stateMachine.ChangeState(new IdleState());
        }
    }
    
    public override bool CanMove()
    {
        return false;
    }
}

public class CrouchPunchState : FighterState
{
    public override void Enter(FighterController fighter, FighterStateMachine stateMachine)
    {
        base.Enter(fighter, stateMachine);
    }
    
    public override void Execute(FighterController fighter, FighterStateMachine stateMachine)
    {
        base.Execute(fighter, stateMachine);
        
        // State is complete
        if (IsAnimationFinished(STARTUP_FRAMES + ACTIVE_FRAMES + RECOVERY_FRAMES))
        {
            // Check if still crouching
            if (fighter.IsCrouching)
                stateMachine.ChangeState(new CrouchState());
            else
                stateMachine.ChangeState(new IdleState());
        }
    }
    
    public override bool CanMove()
    {
        return false;
    }
}

public class CrouchKickState : FighterState
{
    public override void Enter(FighterController fighter, FighterStateMachine stateMachine)
    {
        base.Enter(fighter, stateMachine);
    }
    
    public override void Execute(FighterController fighter, FighterStateMachine stateMachine)
    {
        base.Execute(fighter, stateMachine);
        
        // State is complete
        if (IsAnimationFinished(STARTUP_FRAMES + ACTIVE_FRAMES + RECOVERY_FRAMES))
        {
            // Check if still crouching
            if (fighter.IsCrouching)
                stateMachine.ChangeState(new CrouchState());
            else
                stateMachine.ChangeState(new IdleState());
        }
    }
    
    public override bool CanMove()
    {
        return false;
    }
}

public class JumpKickState : FighterState
{
    public override void Enter(FighterController fighter, FighterStateMachine stateMachine)
    {
        base.Enter(fighter, stateMachine);
    }
    
    public override void Execute(FighterController fighter, FighterStateMachine stateMachine)
    {
        base.Execute(fighter, stateMachine);
        
        // Return to jump loop state when attack is complete
        if (IsAnimationFinished(STARTUP_FRAMES + ACTIVE_FRAMES + RECOVERY_FRAMES))
        {
            // If grounded, go to idle, otherwise loop state
            if (fighter.IsGrounded)
            {
                stateMachine.ChangeState(new IdleState());
            }
            else
            {
                stateMachine.ChangeState(new JumpLoopState());
            }
        }
    }
    
    public override bool CanMove()
    {
        return false;
    }
}

public class JumpPunchState : FighterState
{
    public override void Enter(FighterController fighter, FighterStateMachine stateMachine)
    {
        base.Enter(fighter, stateMachine);
    }
    
    public override void Execute(FighterController fighter, FighterStateMachine stateMachine)
    {
        base.Execute(fighter, stateMachine);
        
        // Return to jump loop state when attack is complete
        if (IsAnimationFinished(STARTUP_FRAMES + ACTIVE_FRAMES + RECOVERY_FRAMES))
        {
            // If grounded, go to idle, otherwise loop state
            if (fighter.IsGrounded)
            {
                stateMachine.ChangeState(new IdleState());
            }
            else
            {
                stateMachine.ChangeState(new JumpLoopState());
            }
        }
    }
    
    public override bool CanMove()
    {
        return false;
    }
}

public class SpecialMoveState : FighterState
{
    private const float SPECIAL_DURATION = 0.5f;
    
    public override void Enter(FighterController fighter, FighterStateMachine stateMachine)
    {
        base.Enter(fighter, stateMachine);
    }
    
    public override void Execute(FighterController fighter, FighterStateMachine stateMachine)
    {
        base.Execute(fighter, stateMachine);
        
        // State is complete
        if (IsAnimationFinished(SPECIAL_DURATION))
            stateMachine.ChangeState(new IdleState());
    }
    
    public override bool CanMove()
    {
        return false;
    }
}

public class HitState : FighterState
{
    private float hitStunDuration = 0.3f;
    
    public override void Enter(FighterController fighter, FighterStateMachine stateMachine)
    {
        base.Enter(fighter, stateMachine);
    }
    
    public override void Execute(FighterController fighter, FighterStateMachine stateMachine)
    {
        base.Execute(fighter, stateMachine);
        
        // State is complete
        if (IsAnimationFinished(hitStunDuration))
            stateMachine.ChangeState(new IdleState());
    }
    
    public override bool CanMove()
    {
        return false;
    }
} 