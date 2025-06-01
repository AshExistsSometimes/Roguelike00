using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController characterController;

    [Header("References")]
    public Camera mainCamera;

    [Header("Speed Settings")]
    public float WalkSpeed = 6f;
    public float SprintSpeed = 8f;

    [Header("Rotation")]
    public float TurnSmoothing = 0.1f;
    private float turnSmoothVelocity;

    [Header("Gravity")]
    public float gravity = 9.81f;
    public float maxFallSpeed = -10f;

    private float currentSpeed;
    private float verticalVelocity;

    private void Start()
    {
        currentSpeed = WalkSpeed;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Update()
    {

        Vector3 direction = GetMovementInput();

        ApplyGravity();

        if (direction.magnitude >= 0.1f)
        {
            MoveAndRotate(direction);
        }
        else
        {
            FaceMouseDirection();
        }

        HandleSprint();
    }

    private Vector3 GetMovementInput()
    {
        float horiz = Input.GetAxisRaw("Horizontal");
        float vert = Input.GetAxisRaw("Vertical");
        return new Vector3(horiz, 0f, vert).normalized;
    }

    private void MoveAndRotate(Vector3 direction)
    {
        float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + mainCamera.transform.eulerAngles.y;
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, TurnSmoothing);
        transform.rotation = Quaternion.Euler(0f, angle, 0f);

        Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
        characterController.Move(moveDir.normalized * currentSpeed * Time.deltaTime);
    }

    private void FaceMouseDirection()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            Vector3 lookDir = hit.point - transform.position;
            lookDir.y = 0f;

            if (lookDir.magnitude > 0.1f)
            {
                Quaternion lookRotation = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
            }
        }
    }

    private void HandleSprint()
    {
        if (PlayerInputManager.Instance.SprintKey)
        {
            currentSpeed = SprintSpeed;
        }
        else
        {
            currentSpeed = WalkSpeed;
        }
    }

    private void ApplyGravity()
    {
        if (characterController.isGrounded)
        {
            // Small downward force to keep player grounded on slopes
            verticalVelocity = gravity * Time.deltaTime;
        }
        else
        {
            // Apply gravity each frame when airborne
            verticalVelocity += gravity * Time.deltaTime;
            verticalVelocity = Mathf.Max(verticalVelocity, maxFallSpeed);
        }

        Vector3 verticalMove = new Vector3(0, verticalVelocity, 0);
        characterController.Move(verticalMove * Time.deltaTime);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject spawnPoint = GameObject.FindWithTag("PlayerSpawnPoint");
        if (spawnPoint != null)
        {
            characterController.enabled = false; // Disable to safely move without physics issues
            transform.position = spawnPoint.transform.position;
            characterController.enabled = true;
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

}
