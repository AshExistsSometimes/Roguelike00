using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

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
    private Transform currentTarget;

    private Coroutine overrideRoutine;


    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void Start()
    {
        currentOffset = defaultPositionOffset;
        targetRotation = Quaternion.Euler(defaultRotationEuler);
        transform.rotation = targetRotation;

        if (playerTarget == null && PlayerSingleton.Instance != null)
            playerTarget = PlayerSingleton.Instance.transform;

        currentTarget = playerTarget;
    }

    private void LateUpdate()
    {
        if (!currentTarget) return;

        Vector3 targetPosition = currentTarget.position + currentOffset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, followSmoothTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
    }

    public void SetCameraOverride(Vector3 newOffset, Vector3 newRotationEuler, Transform overrideTarget = null, float duration = 0f)
    {
        currentOffset = newOffset;
        targetRotation = Quaternion.Euler(newRotationEuler);
        currentTarget = overrideTarget != null ? overrideTarget : playerTarget;

        if (overrideRoutine != null)
            StopCoroutine(overrideRoutine);

        if (duration > 0f)
            overrideRoutine = StartCoroutine(ResetAfterDelay(duration));
    }

    public void ReturnToDefaultCameraAngle()
    {
        currentOffset = defaultPositionOffset;
        targetRotation = Quaternion.Euler(defaultRotationEuler);
        currentTarget = playerTarget;
    }

    private IEnumerator ResetAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnToDefaultCameraAngle();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ReturnToDefaultCameraAngle();
    }
}