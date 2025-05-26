using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

#if UNITY_EDITOR
public class FighterAnimatorControllerCreator
{
    [MenuItem("Animation Tools/Create Fighter Animator Controller")]
    public static void CreateFighterAnimatorController()
    {
        // Create animator controller
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(
            "Assets/Art/Animations/FighterAnimatorController.controller");
            
        // Add parameters
        controller.AddParameter("HorizontalSpeed", AnimatorControllerParameterType.Float);
        controller.AddParameter("IsCrouching", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsBlocking", AnimatorControllerParameterType.Bool);
        controller.AddParameter("JumpStart", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("JumpLoop", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Punch", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Kick", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Special", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Hit", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Dash", AnimatorControllerParameterType.Trigger);
        
        // Add layers
        AnimatorControllerLayer baseLayer = controller.layers[0];
        baseLayer.name = "Base Layer";
        baseLayer.stateMachine.name = "Fighter State Machine";
        
        // Create base states
        AnimatorState idleState = baseLayer.stateMachine.AddState("Idle");
        AnimatorState walkState = baseLayer.stateMachine.AddState("Walk");
        AnimatorState jumpStartState = baseLayer.stateMachine.AddState("JumpStart", new Vector3(0, 200, 0));
        AnimatorState jumpLoopState = baseLayer.stateMachine.AddState("JumpLoop", new Vector3(200, 200, 0));
        AnimatorState crouchState = baseLayer.stateMachine.AddState("Crouch");
        AnimatorState blockState = baseLayer.stateMachine.AddState("Block");
       //hey 
        // Create attack states
        AnimatorState punchState = baseLayer.stateMachine.AddState("Punch");
        AnimatorState kickState = baseLayer.stateMachine.AddState("Kick");
        AnimatorState crouchPunchState = baseLayer.stateMachine.AddState("CrouchPunch");
        AnimatorState crouchKickState = baseLayer.stateMachine.AddState("CrouchKick");
        AnimatorState jumpPunchState = baseLayer.stateMachine.AddState("JumpPunch");
        AnimatorState jumpKickState = baseLayer.stateMachine.AddState("JumpKick");
        AnimatorState specialState = baseLayer.stateMachine.AddState("Special");
        AnimatorState hitState = baseLayer.stateMachine.AddState("Hit");
        AnimatorState dashState = baseLayer.stateMachine.AddState("Dash");
        
        // Set idle as default state
        baseLayer.stateMachine.defaultState = idleState;
        
        // Create transitions
        
        // Idle to other states
        CreateTransition(idleState, walkState, "HorizontalSpeed", AnimatorConditionMode.Greater, 0.1f);
        CreateTransition(idleState, jumpStartState, "JumpStart");
        CreateTransition(idleState, crouchState, "IsCrouching", AnimatorConditionMode.If);
        CreateTransition(idleState, blockState, "IsBlocking", AnimatorConditionMode.If);
        CreateTransition(idleState, punchState, "Punch");
        CreateTransition(idleState, kickState, "Kick");
        CreateTransition(idleState, dashState, "Dash");
        
        // Walk to other states
        CreateTransition(walkState, idleState, "HorizontalSpeed", AnimatorConditionMode.Less, 0.1f);
        CreateTransition(walkState, jumpStartState, "JumpStart");
        CreateTransition(walkState, crouchState, "IsCrouching", AnimatorConditionMode.If);
        CreateTransition(walkState, blockState, "IsBlocking", AnimatorConditionMode.If);
        CreateTransition(walkState, punchState, "Punch");
        CreateTransition(walkState, kickState, "Kick");
        CreateTransition(walkState, dashState, "Dash");
        
        // Jump state transitions
        var idleToJumpStart = idleState.AddTransition(jumpStartState);
        idleToJumpStart.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0, "JumpStart");
        
        var jumpStartToLoop = jumpStartState.AddTransition(jumpLoopState);
        jumpStartToLoop.hasExitTime = true;
        jumpStartToLoop.exitTime = 0.9f;
        jumpStartToLoop.duration = 0.1f;
        
        var jumpStartToLoopDirect = jumpStartState.AddTransition(jumpLoopState);
        jumpStartToLoopDirect.hasExitTime = false;
        jumpStartToLoopDirect.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0, "JumpLoop");
        jumpStartToLoopDirect.duration = 0.1f;
        
        var jumpLoopToIdle = jumpLoopState.AddTransition(idleState);
        jumpLoopToIdle.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0, "IsGrounded");
        jumpLoopToIdle.duration = 0.1f;
        
        var anyStateToJumpLoop = baseLayer.stateMachine.AddAnyStateTransition(jumpLoopState);
        anyStateToJumpLoop.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0, "JumpLoop");
        anyStateToJumpLoop.AddCondition(UnityEditor.Animations.AnimatorConditionMode.IfNot, 0, "IsGrounded");
        anyStateToJumpLoop.duration = 0.1f;
        
        // Add mid-air crouching transitions
        var jumpLoopToCrouch = jumpLoopState.AddTransition(crouchState);
        jumpLoopToCrouch.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0, "IsCrouching");
        jumpLoopToCrouch.duration = 0.1f;
        
        var jumpKickToCrouch = jumpKickState.AddTransition(crouchState);
        jumpKickToCrouch.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0, "IsCrouching");
        jumpKickToCrouch.duration = 0.1f;
        
        var jumpPunchToCrouch = jumpPunchState.AddTransition(crouchState);
        jumpPunchToCrouch.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0, "IsCrouching");
        jumpPunchToCrouch.duration = 0.1f;
        
        // Jump attack transitions
        var jumpLoopToJumpKick = jumpLoopState.AddTransition(jumpKickState);
        jumpLoopToJumpKick.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0, "Kick");
        
        var jumpLoopToJumpPunch = jumpLoopState.AddTransition(jumpPunchState);
        jumpLoopToJumpPunch.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0, "Punch");
        
        var jumpKickToLoop = jumpKickState.AddTransition(jumpLoopState);
        jumpKickToLoop.hasExitTime = true;
        jumpKickToLoop.exitTime = 0.9f;
        jumpKickToLoop.duration = 0.1f;
        
        var jumpPunchToLoop = jumpPunchState.AddTransition(jumpLoopState);
        jumpPunchToLoop.hasExitTime = true;
        jumpPunchToLoop.exitTime = 0.9f;
        jumpPunchToLoop.duration = 0.1f;
        
        // Add grounded transitions for jump attacks
        var jumpKickToIdle = jumpKickState.AddTransition(idleState);
        jumpKickToIdle.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0, "IsGrounded");
        jumpKickToIdle.duration = 0.1f;
        
        var jumpPunchToIdle = jumpPunchState.AddTransition(idleState);
        jumpPunchToIdle.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0, "IsGrounded");
        jumpPunchToIdle.duration = 0.1f;
        
        // Crouch to other states
        CreateTransition(crouchState, idleState, "IsCrouching", AnimatorConditionMode.IfNot);
        CreateTransition(crouchState, crouchPunchState, "Punch");
        CreateTransition(crouchState, crouchKickState, "Kick");
        
        // Attack states back to idle when done
        SetStateToReturnToIdle(punchState);
        SetStateToReturnToIdle(kickState);
        SetStateToReturnToIdle(crouchPunchState);
        SetStateToReturnToIdle(crouchKickState);
        SetStateToReturnToIdle(jumpPunchState);
        SetStateToReturnToIdle(jumpKickState);
        SetStateToReturnToIdle(specialState);
        SetStateToReturnToIdle(hitState);
        SetStateToReturnToIdle(dashState);
        
        // Return from block
        CreateTransition(blockState, idleState, "IsBlocking", AnimatorConditionMode.IfNot);
        
        Debug.Log("Fighter Animator Controller created at: Assets/Art/Animations/FighterAnimatorController.controller");
    }
    
    private static AnimatorStateTransition CreateTransition(
        AnimatorState sourceState, 
        AnimatorState destinationState, 
        string parameterName, 
        AnimatorConditionMode conditionMode = AnimatorConditionMode.If, 
        float threshold = 0f)
    {
        AnimatorStateTransition transition = sourceState.AddTransition(destinationState);
        transition.AddCondition(conditionMode, threshold, parameterName);
        transition.hasExitTime = false;
        transition.duration = 0.1f;
        return transition;
    }
    
    private static void SetStateToReturnToIdle(AnimatorState state)
    {
        AnimatorStateTransition transition = state.AddExitTransition();
        transition.hasExitTime = true;
        transition.exitTime = 0.9f; // Return to idle when animation is 90% complete
        transition.duration = 0.1f;
        transition.destinationState = null; // This makes it an exit transition
    }
}
#endif 