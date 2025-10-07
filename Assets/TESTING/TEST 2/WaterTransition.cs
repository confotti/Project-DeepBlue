using UnityEngine;

public class CameraCullingMaskTrigger : MonoBehaviour
{
    public Camera targetCamera;          // Assign your main camera here
    private int originalMask;            // Store the mask to restore later

    void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera != null)
            originalMask = targetCamera.cullingMask;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (targetCamera == null) return;

        // You can check if "other" is the player here if needed:
        // if (!other.CompareTag("Player")) return;

        targetCamera.cullingMask = ~0;   // This sets it to "Everything"
    }
}