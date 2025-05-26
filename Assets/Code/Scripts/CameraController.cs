using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private float minSize = 4f;
    [SerializeField] private float maxSize = 12f;  // Increased to allow more zoom out

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float zoomSpeed = 3f;
    
    [Header("Fighting Game Camera Settings")]
    [SerializeField] private float minimumBottomY = -3f;  // Camera bottom boundary
    [SerializeField] private float verticalMargin = 1.5f;  // Extra space above characters
    [SerializeField] private float horizontalMargin = 2f;  // Extra space on sides
    
    private Transform target1;
    private Transform target2;
    private Camera mainCamera;
    
    private void Awake()
    {
        mainCamera = GetComponent<Camera>();
        
        if (mainCamera == null)
        {
            Debug.LogError("CameraController: No camera component found!");
            enabled = false;
        }
    }
    
    public void SetTargets(Transform first, Transform second)
    {
        target1 = first;
        target2 = second;
        
        // Set initial camera position if both targets are available
        if (target1 != null && target2 != null)
        {
            UpdateCameraPosition();
        }
    }
    
    private void LateUpdate()
    {
        if (target1 == null || target2 == null)
            return;
            
        UpdateCameraPosition();
    }
    
    private void UpdateCameraPosition()
    {
        // Calculate bounds that include both characters
        float leftMost = Mathf.Min(target1.position.x, target2.position.x) - horizontalMargin;
        float rightMost = Mathf.Max(target1.position.x, target2.position.x) + horizontalMargin;
        float bottomMost = Mathf.Min(target1.position.y, target2.position.y);
        float topMost = Mathf.Max(target1.position.y, target2.position.y) + verticalMargin;
        
        // Calculate required width and height to contain both characters
        float requiredWidth = rightMost - leftMost;
        float requiredHeight = topMost - bottomMost;
        
        // Calculate center point between characters
        float centerX = (leftMost + rightMost) * 0.5f;
        float centerY = (bottomMost + topMost) * 0.5f;
        
        // Calculate required orthographic size based on both width and height
        float aspectRatio = mainCamera.aspect;
        float sizeForWidth = requiredWidth / (2f * aspectRatio);
        float sizeForHeight = requiredHeight / 2f;
        float requiredSize = Mathf.Max(sizeForWidth, sizeForHeight);
        
        // Clamp the size within our limits
        requiredSize = Mathf.Clamp(requiredSize, minSize, maxSize);
        
        // Check if the camera bottom would go below our minimum boundary
        float cameraBottom = centerY - requiredSize;
        
        if (cameraBottom < minimumBottomY)
        {
            // Adjust camera Y position to keep bottom at minimum level
            centerY = minimumBottomY + requiredSize;
            
            // If characters are still not fully visible, we need to zoom out more
            float adjustedTopMost = topMost;
            float cameraTop = centerY + requiredSize;
            
            if (adjustedTopMost > cameraTop)
            {
                // Need to zoom out to fit the top character
                float neededSizeForTop = (adjustedTopMost - minimumBottomY) / 2f;
                requiredSize = Mathf.Clamp(neededSizeForTop, minSize, maxSize);
                centerY = minimumBottomY + requiredSize;
            }
        }
        
        // Target camera position
        Vector3 targetPosition = new Vector3(centerX, centerY, transform.position.z);
        
        // Move camera with smoothing
        transform.position = Vector3.Lerp(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        
        // Apply size with smoothing
        if (mainCamera.orthographic)
        {
            mainCamera.orthographicSize = Mathf.Lerp(
                mainCamera.orthographicSize, 
                requiredSize, 
                zoomSpeed * Time.deltaTime);
        }
        else
        {
            // For perspective camera, adjust FOV
            float targetFOV = Mathf.Clamp(requiredSize * 8f + 30f, 30f, 80f);
            mainCamera.fieldOfView = Mathf.Lerp(
                mainCamera.fieldOfView,
                targetFOV,
                zoomSpeed * Time.deltaTime);
        }
    }
} 