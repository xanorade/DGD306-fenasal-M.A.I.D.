using UnityEngine;

public class SceneCameraSetup : MonoBehaviour
{
    void Start()
    {
        var fighters = FindObjectsOfType<FighterController>();
        if (fighters.Length >= 2)
        {
            Camera.main.GetComponent<CameraController>().SetTargets(fighters[0].transform, fighters[1].transform);
        }
    }
}