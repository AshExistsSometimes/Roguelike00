using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTriggerZone : MonoBehaviour
{
    public Vector3 newOffset;
    public Vector3 newRotationEuler;
    public Transform overrideTarget;         // Optional target to look at
    public float overrideDuration = 0f;      // Duration before returning to default
    public bool runOnce = false;
    public bool returnToDefaultOnExit = true;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered && runOnce) return;
        if (!other.CompareTag("Player")) return;

        var camController = Camera.main.GetComponent<CameraController>();
        camController.SetCameraOverride(newOffset, newRotationEuler, overrideTarget, overrideDuration);

        if (runOnce)
            hasTriggered = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!returnToDefaultOnExit || runOnce) return;
        if (other.CompareTag("Player"))
        {
            var camController = Camera.main.GetComponent<CameraController>();
            camController.ReturnToDefaultCameraAngle();
        }
    }
}
