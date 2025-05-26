using UnityEngine;
using UnityEditor;
using System.IO;

#if UNITY_EDITOR
namespace DGD306.Character
{
    [ExecuteInEditMode]
    public class FighterAnimationSetup : MonoBehaviour
    {
        [Header("Animation Setup")]
        [SerializeField] private RuntimeAnimatorController animatorController;
        
        [Header("Animation Clips")]
        [SerializeField] private AnimationClip idleAnimation;
        [SerializeField] private AnimationClip walkAnimation;
        [SerializeField] private AnimationClip jumpStartAnimation;
        [SerializeField] private AnimationClip jumpLoopAnimation;
        [SerializeField] private AnimationClip crouchAnimation;
        [SerializeField] private AnimationClip kickAnimation;
        [SerializeField] private AnimationClip punchAnimation;
        [SerializeField] private AnimationClip blockAnimation;
        [SerializeField] private AnimationClip hitAnimation;
        [SerializeField] private AnimationClip dashAnimation;
        [SerializeField] private AnimationClip specialAnimation;
        
        [Header("Special Attacks")]
        [SerializeField] private AnimationClip crouchPunchAnimation;
        [SerializeField] private AnimationClip crouchKickAnimation;
        [SerializeField] private AnimationClip jumpPunchAnimation;
        [SerializeField] private AnimationClip jumpKickAnimation;
        
        private Animator animator;
        
        private void Awake()
        {
            animator = GetComponent<Animator>();
        }
        
        public void SetupAnimator()
        {
            if (animator == null)
                animator = gameObject.AddComponent<Animator>();
                
            if (animatorController != null)
                animator.runtimeAnimatorController = animatorController;
            
            // Make sure FighterController exists
            FighterController controller = GetComponent<FighterController>();
            if (controller == null)
                controller = gameObject.AddComponent<FighterController>();
            
            // Add sprite renderer if missing
            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            if (renderer == null)
                renderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        [ContextMenu("Apply Animations to Controller")]
        public void ApplyAnimationsToController()
        {
            if (animatorController == null)
            {
                Debug.LogError("No Animator Controller assigned!");
                return;
            }
            
            // Get the animator controller
            UnityEditor.Animations.AnimatorController controller = 
                animatorController as UnityEditor.Animations.AnimatorController;
                
            if (controller == null)
            {
                Debug.LogError("Failed to cast to AnimatorController!");
                return;
            }
            
            // Get all states in the controller
            UnityEditor.Animations.AnimatorControllerLayer baseLayer = controller.layers[0];
            UnityEditor.Animations.AnimatorStateMachine stateMachine = baseLayer.stateMachine;
            
            // Assign animations to states
            foreach (UnityEditor.Animations.ChildAnimatorState state in stateMachine.states)
            {
                AssignAnimationToState(state.state);
            }
            
            // Save changes
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            
            Debug.Log("Successfully applied animations to controller states!");
        }
        
        private void AssignAnimationToState(UnityEditor.Animations.AnimatorState state)
        {
            switch (state.name)
            {
                case "Idle":
                    if (idleAnimation != null) state.motion = idleAnimation;
                    break;
                case "Walk":
                    if (walkAnimation != null) state.motion = walkAnimation;
                    break;
                case "JumpStart":
                    if (jumpStartAnimation != null) 
                    {
                        state.motion = jumpStartAnimation;
                        Debug.Log($"Assigned jump start animation to JumpStart state: {jumpStartAnimation.name}");
                    }
                    else
                    {
                        Debug.LogWarning("Jump start animation is not assigned!");
                    }
                    break;
                case "JumpLoop":
                    if (jumpLoopAnimation != null) 
                    {
                        state.motion = jumpLoopAnimation;
                        Debug.Log($"Assigned jump loop animation to JumpLoop state: {jumpLoopAnimation.name}");
                    }
                    else
                    {
                        Debug.LogWarning("Jump loop animation is not assigned!");
                    }
                    break;
                case "Crouch":
                    if (crouchAnimation != null) state.motion = crouchAnimation;
                    break;
                case "Kick":
                    if (kickAnimation != null) state.motion = kickAnimation;
                    break;
                case "Punch":
                    if (punchAnimation != null) state.motion = punchAnimation;
                    break;
                case "Block":
                    if (blockAnimation != null) state.motion = blockAnimation;
                    break;
                case "Hit":
                    if (hitAnimation != null) state.motion = hitAnimation;
                    break;
                case "Dash":
                    if (dashAnimation != null) state.motion = dashAnimation;
                    break;
                case "Special":
                    if (specialAnimation != null) state.motion = specialAnimation;
                    break;
                case "CrouchPunch":
                    if (crouchPunchAnimation != null) state.motion = crouchPunchAnimation;
                    break;
                case "CrouchKick":
                    if (crouchKickAnimation != null) state.motion = crouchKickAnimation;
                    break;
                case "JumpPunch":
                    if (jumpPunchAnimation != null) state.motion = jumpPunchAnimation;
                    break;
                case "JumpKick":
                    if (jumpKickAnimation != null) state.motion = jumpKickAnimation;
                    break;
            }
        }
    }
    
    [CustomEditor(typeof(FighterAnimationSetup))]
    public class FighterAnimationSetupEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            FighterAnimationSetup setup = (FighterAnimationSetup)target;
            
            EditorGUILayout.Space(10);
            
            if (GUILayout.Button("Setup Animator"))
            {
                setup.SetupAnimator();
            }
            
            if (GUILayout.Button("Apply Animations to Controller"))
            {
                setup.ApplyAnimationsToController();
            }
        }
    }
}
#endif 