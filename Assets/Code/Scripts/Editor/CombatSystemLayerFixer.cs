using UnityEngine;
using UnityEditor;
using DGD306.Character;

#if UNITY_EDITOR
public class CombatSystemLayerFixer : EditorWindow
{
    [MenuItem("Combat Tools/Fix Combat System Layers")]
    public static void ShowWindow()
    {
        GetWindow<CombatSystemLayerFixer>("Combat Layer Fixer");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Combat System Layer Configuration", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "This tool will help fix the layer configuration for your combat system.\n\n" +
            "First, make sure you have these layers set up:\n" +
            "• Layer 8: Hitboxes\n" +
            "• Layer 9: Hurtboxes\n" +
            "• Layer 10: Players", 
            MessageType.Info);
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Check Current Layer Setup"))
        {
            CheckLayerSetup();
        }
        
        GUILayout.Space(5);
        
        if (GUILayout.Button("Fix All CombatSystemSetup Components"))
        {
            FixAllCombatSystemSetups();
        }
        
        GUILayout.Space(5);
        
        if (GUILayout.Button("Setup Missing Layers Automatically"))
        {
            SetupLayers();
        }
        
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "After running these fixes:\n" +
            "1. Select your fighter prefabs in the scene\n" +
            "2. Check that the CombatSystemSetup layer masks are properly configured\n" +
            "3. Make sure Gizmos are enabled in the Scene view\n" +
            "4. Enter Play mode and perform attacks to see the hitboxes",
            MessageType.Warning);
    }
    
    private void CheckLayerSetup()
    {
        Debug.Log("=== Combat System Layer Check ===");
        
        // Check if required layers exist
        bool layer8Exists = !string.IsNullOrEmpty(LayerMask.LayerToName(8));
        bool layer9Exists = !string.IsNullOrEmpty(LayerMask.LayerToName(9));
        bool layer10Exists = !string.IsNullOrEmpty(LayerMask.LayerToName(10));
        
        Debug.Log($"Layer 8 (Hitboxes): {(layer8Exists ? LayerMask.LayerToName(8) : "NOT SET")}");
        Debug.Log($"Layer 9 (Hurtboxes): {(layer9Exists ? LayerMask.LayerToName(9) : "NOT SET")}");
        Debug.Log($"Layer 10 (Players): {(layer10Exists ? LayerMask.LayerToName(10) : "NOT SET")}");
        
        // Check CombatSystemSetup components
        CombatSystemSetup[] combatSetups = FindObjectsOfType<CombatSystemSetup>();
        Debug.Log($"Found {combatSetups.Length} CombatSystemSetup components:");
        
        foreach (var setup in combatSetups)
        {
            var setupScript = setup as CombatSystemSetup;
            var layerSetupField = typeof(CombatSystemSetup).GetField("layerSetup", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (layerSetupField != null)
            {
                var layerSetup = layerSetupField.GetValue(setupScript);
                var hitboxLayersField = layerSetup.GetType().GetField("hitboxLayers");
                var hurtboxLayersField = layerSetup.GetType().GetField("hurtboxLayers");
                var playerLayersField = layerSetup.GetType().GetField("playerLayers");
                
                if (hitboxLayersField != null && hurtboxLayersField != null && playerLayersField != null)
                {
                    LayerMask hitboxLayers = (LayerMask)hitboxLayersField.GetValue(layerSetup);
                    LayerMask hurtboxLayers = (LayerMask)hurtboxLayersField.GetValue(layerSetup);
                    LayerMask playerLayers = (LayerMask)playerLayersField.GetValue(layerSetup);
                    
                    Debug.Log($"  {setup.gameObject.name}:");
                    Debug.Log($"    Hitbox Layers: {hitboxLayers.value}");
                    Debug.Log($"    Hurtbox Layers: {hurtboxLayers.value}");
                    Debug.Log($"    Player Layers: {playerLayers.value}");
                }
            }
        }
    }
    
    private void FixAllCombatSystemSetups()
    {
        CombatSystemSetup[] combatSetups = FindObjectsOfType<CombatSystemSetup>();
        int fixedCount = 0;
        
        foreach (var setup in combatSetups)
        {
            var setupScript = setup as CombatSystemSetup;
            var layerSetupField = typeof(CombatSystemSetup).GetField("layerSetup", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (layerSetupField != null)
            {
                var layerSetup = layerSetupField.GetValue(setupScript);
                var hitboxLayersField = layerSetup.GetType().GetField("hitboxLayers");
                var hurtboxLayersField = layerSetup.GetType().GetField("hurtboxLayers");
                var playerLayersField = layerSetup.GetType().GetField("playerLayers");
                
                if (hitboxLayersField != null && hurtboxLayersField != null && playerLayersField != null)
                {
                    // Set the correct layer values
                    hitboxLayersField.SetValue(layerSetup, (LayerMask)(1 << 8)); // Layer 8
                    hurtboxLayersField.SetValue(layerSetup, (LayerMask)(1 << 9)); // Layer 9  
                    playerLayersField.SetValue(layerSetup, (LayerMask)(1 << 10)); // Layer 10
                    
                    // Mark the object as dirty so Unity saves the changes
                    EditorUtility.SetDirty(setup);
                    
                    fixedCount++;
                    Debug.Log($"Fixed layer configuration for {setup.gameObject.name}");
                }
            }
        }
        
        Debug.Log($"Fixed {fixedCount} CombatSystemSetup components!");
        
        // Also run the setup to apply the changes
        foreach (var setup in combatSetups)
        {
            setup.SetupCombatSystem();
        }
    }
    
    private void SetupLayers()
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layersProp = tagManager.FindProperty("layers");
        
        SetLayerName(layersProp, 8, "Hitboxes");
        SetLayerName(layersProp, 9, "Hurtboxes");
        SetLayerName(layersProp, 10, "Players");
        
        tagManager.ApplyModifiedProperties();
        
        Debug.Log("Layer setup complete! Layers 8-10 have been configured.");
    }
    
    private void SetLayerName(SerializedProperty layersProp, int layerIndex, string layerName)
    {
        SerializedProperty layerProp = layersProp.GetArrayElementAtIndex(layerIndex);
        if (string.IsNullOrEmpty(layerProp.stringValue))
        {
            layerProp.stringValue = layerName;
            Debug.Log($"Set layer {layerIndex} to '{layerName}'");
        }
        else
        {
            Debug.Log($"Layer {layerIndex} already set to '{layerProp.stringValue}'");
        }
    }
}
#endif