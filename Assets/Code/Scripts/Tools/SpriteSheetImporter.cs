using UnityEngine;
using UnityEditor;
using UnityEditor.U2D;
using UnityEditor.U2D.Sprites;
using System.IO;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
public class SpriteSheetImporter : EditorWindow
{
    private Object spriteSheet;
    private string animationName = "NewAnimation";
    private int frameCount = 4;
    private float framesPerSecond = 12;
    private bool createAnimatorController = false;
    private string saveFolder = "Assets/Art/Animations";
    private float pixelsPerUnit = 2.0f;
    private bool loopAnimation = true;
    private bool isJumpAnimation = false;
    private int midAirFrame = 1; // Default to second frame (0-based index)
    
    [MenuItem("Animation Tools/Sprite Sheet Animation Tool")]
    public static void ShowWindow()
    {
        GetWindow<SpriteSheetImporter>("Sprite Sheet Importer");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Sprite Sheet Animation Importer", EditorStyles.boldLabel);
        
        spriteSheet = EditorGUILayout.ObjectField("Sprite Sheet", spriteSheet, typeof(Texture2D), false);
        animationName = EditorGUILayout.TextField("Animation Name", animationName);
        frameCount = EditorGUILayout.IntField("Frame Count", frameCount);
        framesPerSecond = EditorGUILayout.FloatField("Frames Per Second", framesPerSecond);
        pixelsPerUnit = EditorGUILayout.FloatField("Pixels Per Unit", pixelsPerUnit);
        loopAnimation = EditorGUILayout.Toggle("Loop Animation", loopAnimation);
        createAnimatorController = EditorGUILayout.Toggle("Create Animator Controller", createAnimatorController);
        saveFolder = EditorGUILayout.TextField("Save Folder", saveFolder);
        
        EditorGUILayout.Space(10);
        isJumpAnimation = EditorGUILayout.Toggle("Is Jump Animation", isJumpAnimation);
        
        if (isJumpAnimation)
        {
            midAirFrame = EditorGUILayout.IntSlider("Mid-Air Frame", midAirFrame, 0, frameCount - 1);
            EditorGUILayout.HelpBox("For jump animations, the mid-air frame will be held longer to create a proper jump arc.", MessageType.Info);
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Create Animation Clip"))
        {
            if (spriteSheet != null)
            {
                CreateAnimationFromSpriteSheet();
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Please assign a sprite sheet texture.", "OK");
            }
        }
        
        if (GUILayout.Button("Fix Existing Sprite Sheet"))
        {
            if (spriteSheet != null)
            {
                FixExistingSpriteSheet();
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Please assign a sprite sheet texture.", "OK");
            }
        }
    }
    
    private void FixExistingSpriteSheet()
    {
        Texture2D texture = spriteSheet as Texture2D;
        if (texture == null) return;
        
        string assetPath = AssetDatabase.GetAssetPath(texture);
        
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer != null)
        {
            importer.spritePixelsPerUnit = pixelsPerUnit;
            
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
            
            EditorUtility.DisplayDialog("Success", $"Updated sprite sheet at {assetPath} with Pixels Per Unit: {pixelsPerUnit}", "OK");
        }
    }
    
    private void CreateAnimationFromSpriteSheet()
    {
        Texture2D texture = spriteSheet as Texture2D;
        if (texture == null) return;
        
        string assetPath = AssetDatabase.GetAssetPath(texture);
        
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            
            importer.spritePixelsPerUnit = pixelsPerUnit;
            
            int frameWidth = texture.width / frameCount;
            int frameHeight = texture.height;
            
            var factory = new SpriteDataProviderFactories();
            var dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);
            dataProvider.InitSpriteEditorDataProvider();
            
            var spriteRects = new List<SpriteRect>();
            for (int i = 0; i < frameCount; i++)
            {
                var spriteRect = new SpriteRect
                {
                    name = $"{animationName}_{i}",
                    rect = new Rect(i * frameWidth, 0, frameWidth, frameHeight),
                    pivot = new Vector2(0.5f, 0.0f)
                };
                spriteRects.Add(spriteRect);
            }
            
            dataProvider.SetSpriteRects(spriteRects.ToArray());
            dataProvider.Apply();
            
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
            
            if (isJumpAnimation)
            {
                CreateJumpAnimations(assetPath);
            }
            else
            {
                CreateAnimationClip(assetPath);
            }
        }
    }
    
    private void CreateJumpAnimations(string spritePath)
    {
        if (!Directory.Exists(saveFolder))
        {
            Directory.CreateDirectory(saveFolder);
        }
        
        // Load the sprites from the sprite sheet
        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(spritePath)
            .OfType<Sprite>()
            .OrderBy(s => s.name)
            .ToArray();
            
        if (sprites.Length <= 1)
        {
            EditorUtility.DisplayDialog("Error", "Failed to load sprites from sheet.", "OK");
            return;
        }
        
        // Create JumpStart animation (first frame)
        AnimationClip startClip = new AnimationClip();
        startClip.frameRate = framesPerSecond;
        CreateSingleFrameAnimation(startClip, sprites[0], $"{animationName}_Start");
        
        // Create JumpLoop animation (mid-air frame) - this should be a static frame
        AnimationClip loopClip = new AnimationClip();
        loopClip.frameRate = framesPerSecond;
        CreateSingleFrameAnimation(loopClip, sprites[midAirFrame], $"{animationName}_Loop");
        
        // No looping needed - just hold this single frame
        // We'll let the state machine handle staying in this state
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        EditorUtility.DisplayDialog("Success", 
            $"Created jump animations:\n" +
            $"{saveFolder}/{animationName}_Start.anim\n" +
            $"{saveFolder}/{animationName}_Loop.anim", 
            "OK");
    }
    
    private void CreateSingleFrameAnimation(AnimationClip clip, Sprite sprite, string clipName)
    {
        EditorCurveBinding spriteBinding = new EditorCurveBinding();
        spriteBinding.type = typeof(SpriteRenderer);
        spriteBinding.path = "";
        spriteBinding.propertyName = "m_Sprite";
        
        // For a single frame animation, we add two keyframes with the same sprite
        // but at different times to ensure it stays on that frame
        ObjectReferenceKeyframe[] spriteKeyFrames = new ObjectReferenceKeyframe[2];
        
        // First keyframe at time 0
        spriteKeyFrames[0] = new ObjectReferenceKeyframe
        {
            time = 0f,
            value = sprite
        };
        
        // Second keyframe at a later time with the same sprite value
        // This ensures the animation stays on this frame
        spriteKeyFrames[1] = new ObjectReferenceKeyframe
        {
            time = 1f, // 1 second - long enough to ensure it holds
            value = sprite
        };
        
        AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, spriteKeyFrames);
        
        string clipPath = $"{saveFolder}/{clipName}.anim";
        AssetDatabase.CreateAsset(clip, clipPath);
    }
    
    private void CreateAnimationClip(string spritePath)
    {
        if (!Directory.Exists(saveFolder))
        {
            Directory.CreateDirectory(saveFolder);
        }
        
        Object[] sprites = AssetDatabase.LoadAllAssetsAtPath(spritePath);
        if (sprites.Length <= 1)
        {
            EditorUtility.DisplayDialog("Error", "Failed to load sprites from sheet. Make sure sprite slicing worked properly.", "OK");
            return;
        }
        
        AnimationClip clip = new AnimationClip();
        clip.frameRate = framesPerSecond;
        
        if (loopAnimation)
        {
            SerializedObject serializedClip = new SerializedObject(clip);
            AnimationClipSettings clipSettings = new AnimationClipSettings
            {
                loopTime = true
            };
            
            SerializedProperty settingsProp = serializedClip.FindProperty("m_AnimationClipSettings");
            settingsProp.FindPropertyRelative("m_LoopTime").boolValue = true;
            serializedClip.ApplyModifiedProperties();
        }
        
        EditorCurveBinding spriteBinding = new EditorCurveBinding();
        spriteBinding.type = typeof(SpriteRenderer);
        spriteBinding.path = "";
        spriteBinding.propertyName = "m_Sprite";
        
        // For jump animations, we'll create a custom sequence
        if (isJumpAnimation)
        {
            // Calculate total frames needed for the jump animation
            int totalFrames = frameCount + 2; // Add 2 extra frames for mid-air hold
            ObjectReferenceKeyframe[] spriteKeyFrames = new ObjectReferenceKeyframe[totalFrames];
            
            System.Array.Sort(sprites, (a, b) => {
                if (!(a is Sprite) || !(b is Sprite)) return 0;
                return a.name.CompareTo(b.name);
            });
            
            int frameIndex = 0;
            float frameTime = 0f;
            
            // Add frames up to mid-air
            for (int i = 0; i <= midAirFrame; i++)
            {
                if (sprites[i] is Sprite)
                {
                    ObjectReferenceKeyframe keyFrame = new ObjectReferenceKeyframe();
                    keyFrame.time = frameTime;
                    keyFrame.value = sprites[i];
                    spriteKeyFrames[frameIndex++] = keyFrame;
                    frameTime += 1f / framesPerSecond;
                }
            }
            
            // Hold mid-air frame for longer (3x normal duration)
            ObjectReferenceKeyframe midAirKeyFrame = new ObjectReferenceKeyframe();
            midAirKeyFrame.time = frameTime;
            midAirKeyFrame.value = sprites[midAirFrame];
            spriteKeyFrames[frameIndex++] = midAirKeyFrame;
            frameTime += (3f / framesPerSecond); // Hold for 3 frames
            
            // Add remaining frames
            for (int i = midAirFrame + 1; i < frameCount; i++)
            {
                if (sprites[i] is Sprite)
                {
                    ObjectReferenceKeyframe keyFrame = new ObjectReferenceKeyframe();
                    keyFrame.time = frameTime;
                    keyFrame.value = sprites[i];
                    spriteKeyFrames[frameIndex++] = keyFrame;
                    frameTime += 1f / framesPerSecond;
                }
            }
            
            // Create the animation curve
            AnimationCurve spriteCurve = new AnimationCurve();
            for (int i = 0; i < frameIndex; i++)
            {
                spriteCurve.AddKey(new Keyframe(spriteKeyFrames[i].time, i));
            }
            
            // Set the curve
            clip.SetCurve("", typeof(SpriteRenderer), "m_Sprite", spriteCurve);
            
            // Save the animation clip
            string jumpClipPath = Path.Combine(saveFolder, $"{animationName}.anim");
            AssetDatabase.CreateAsset(clip, jumpClipPath);
            AssetDatabase.SaveAssets();
            
            Debug.Log($"Created jump animation at: {jumpClipPath} with {frameIndex} frames");
            return;
        }
        else
        {
            // Regular animation creation
            ObjectReferenceKeyframe[] spriteKeyFrames = new ObjectReferenceKeyframe[frameCount];
            
            System.Array.Sort(sprites, (a, b) => {
                if (!(a is Sprite) || !(b is Sprite)) return 0;
                return a.name.CompareTo(b.name);
            });
            
            int frameIndex = 0;
            for (int i = 0; i < sprites.Length; i++)
            {
                if (sprites[i] is Sprite)
                {
                    if (frameIndex < frameCount)
                    {
                        ObjectReferenceKeyframe keyFrame = new ObjectReferenceKeyframe();
                        keyFrame.time = frameIndex / framesPerSecond;
                        keyFrame.value = sprites[i];
                        spriteKeyFrames[frameIndex] = keyFrame;
                        frameIndex++;
                    }
                }
            }
            
            AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, spriteKeyFrames);
        }
        
        string clipPath = $"{saveFolder}/{animationName}.anim";
        AssetDatabase.CreateAsset(clip, clipPath);
        
        clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
        if (clip != null && loopAnimation)
        {
            SerializedObject serializedClip = new SerializedObject(clip);
            SerializedProperty settingsProp = serializedClip.FindProperty("m_AnimationClipSettings");
            if (settingsProp != null)
            {
                settingsProp.FindPropertyRelative("m_LoopTime").boolValue = true;
                serializedClip.ApplyModifiedProperties();
            }
        }
        
        if (createAnimatorController)
        {
            string controllerPath = $"{saveFolder}/{animationName}_Controller.controller";
            
            UnityEditor.Animations.AnimatorController controller = 
                UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            
            UnityEditor.Animations.AnimatorState state = controller.layers[0].stateMachine.AddState(animationName);
            state.motion = clip;
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        EditorUtility.DisplayDialog("Success", $"Animation created at {clipPath}", "OK");
    }
}

[System.Serializable]
public class AnimationClipSettings
{
    public bool loopTime;
}
#endif 