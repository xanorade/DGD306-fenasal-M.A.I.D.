using UnityEngine;

public class StateTracker : MonoBehaviour
{
    [SerializeField] public FighterController fighter;
    
    private TextMesh textMesh3D;
    private UnityEngine.UI.Text uiText;
    
    private void Start()
    {
        // Try to find a fighter if not set
        if (fighter == null)
            fighter = FindObjectOfType<FighterController>();
            
        // Try to find the text component
        textMesh3D = GetComponent<TextMesh>();
        uiText = GetComponent<UnityEngine.UI.Text>();
        
        if (fighter == null)
            Debug.LogWarning("StateTracker: No fighter found to track.");
    }
    
    private void Update()
    {
        if (fighter != null)
        {
            string stateText = "State: " + fighter.CurrentStateName;
            
            // Update the appropriate text component
            if (textMesh3D != null)
                textMesh3D.text = stateText;
            else if (uiText != null)
                uiText.text = stateText;
        }
    }
} 