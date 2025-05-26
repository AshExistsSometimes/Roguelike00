using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform playerTarget;

    [Header("Camera Offset")]
    public Vector3 positionOffset = new Vector3(0f, 10f, -10f);
    public Vector3 rotationEuler = new Vector3(45f, 0f, 0f);

    [Header("Smoothing")]
    public float followSmoothTime = 0.2f;

    private Vector3 velocity = Vector3.zero;

    private void Start()
    {
        transform.rotation = Quaternion.Euler(rotationEuler);
    }

    private void LateUpdate()
    {
        if (!playerTarget)
        {
            return;
        }

        Vector3 targetPosition = playerTarget.position + positionOffset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, followSmoothTime);
    }
}
