using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTriggerZone : MonoBehaviour
{
    public Vector3 newOffset;
    public Vector3 newRotationEuler;
    public bool returnToDefaultOnExit = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Camera.main.GetComponent<CameraController>().SetCameraOverride(newOffset, newRotationEuler);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (returnToDefaultOnExit && other.CompareTag("Player"))
        {
            Camera.main.GetComponent<CameraController>().ReturnToDefaultCameraAngle();
        }
    }
}
