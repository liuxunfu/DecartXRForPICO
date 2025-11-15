using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera targetCamera;
    
    void Start()
    {
        // Find the main camera or first active camera
        targetCamera = Camera.main;
        if (targetCamera == null)
        {
            targetCamera = FindObjectOfType<Camera>();
        }
    }
    
    void Update()
    {
        if (targetCamera != null)
        {
            transform.LookAt(targetCamera.transform);
            transform.Rotate(0, 180, 0);
        }
    }
}
