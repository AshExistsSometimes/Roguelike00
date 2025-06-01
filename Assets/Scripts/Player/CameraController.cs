using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform playerTarget;

    [Header("Default Camera Offset")]
    public Vector3 defaultPositionOffset = new Vector3(0f, 12f, -11.5f);
    public Vector3 defaultRotationEuler = new Vector3(45f, 0f, 0f);

    [Header("Smoothing")]
    public float followSmoothTime = 0.2f;

    private Vector3 currentOffset;
    private Vector3 velocity = Vector3.zero;
    private Quaternion targetRotation;

    private void Start()
    {
        currentOffset = defaultPositionOffset;
        targetRotation = Quaternion.Euler(defaultRotationEuler);
        transform.rotation = targetRotation;

            if (playerTarget == null && PlayerSingleton.Instance != null)
            {
                playerTarget = PlayerSingleton.Instance.transform;
            }
    }

    private void LateUpdate()
    {
        if (!playerTarget) return;

        Vector3 targetPosition = playerTarget.position + currentOffset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, followSmoothTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
    }

    // Call this from a trigger to change camera position
    public void SetCameraOverride(Vector3 newOffset, Vector3 newRotationEuler)
    {
        currentOffset = newOffset;
        targetRotation = Quaternion.Euler(newRotationEuler);
    }

    // Call this to return to default camera position
    public void ReturnToDefaultCameraAngle()
    {
        currentOffset = defaultPositionOffset;
        targetRotation = Quaternion.Euler(defaultRotationEuler);
    }
}
