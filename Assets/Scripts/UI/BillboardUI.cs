using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardUI : MonoBehaviour
{
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (mainCamera != null)
        {
            // Rotate to face the camera
            Vector3 direction = transform.position - mainCamera.transform.position;
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}
