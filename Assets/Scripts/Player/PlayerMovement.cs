using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerMovement : MonoBehaviour, IDamageable
{
    public CharacterController characterController;

    [Header("References")]
    public Camera mainCamera;
    public Transform modelPosition;

    [Header("Speed Settings")]
    public float WalkSpeed = 6f;
    public float SprintSpeed = 8f;

    [Header("Rotation")]
    public float TurnSmoothing = 0.1f;
    private float turnSmoothVelocity;

    [Header("Gravity")]
    public float gravity = 9.81f;
    public float maxFallSpeed = -10f;

    private float verticalVelocity;

    [Header("Character Stats")]
    public int maxHP;
    public int currentHP;
    [Space]
    public int attackDamage;
    public float moveSpeed;
    private float currentSpeed;

    public float attackSpeed;// Attacks per second
    private float attackCooldown;  // time between attacks
    private float attackTimer = 0f;


    [Header("Attack")]
    public Transform rangedFirePoint; // assign an empty GameObject where projectiles spawn
    public GameObject meleeHitboxObject; // assign the melee hitbox GameObject here
    public float meleeAttackDuration = 0.2f;

    [Header("Character Data")]
    public CharacterData selectedCharacter;
    public CharacterData defaultCharacter;


    [Header("Current Stats")]
    public int CurrentMaxHP;
    [Space]
    public int CurrentDamage;

    public float CurrentAttackSpeed;

    public float CurrentSpeedStat;

    private void Start()
    {
        currentSpeed = WalkSpeed;
        currentHP = maxHP;
        attackCooldown = 1f / attackSpeed;

        SceneManager.sceneLoaded += OnSceneLoaded;

        if (selectedCharacter == null)
        {
            selectedCharacter = defaultCharacter;
        }

        // TEMPORARY UNTIL SET DYNAMICALLY
        CurrentMaxHP = selectedCharacter.baseHP;
        CurrentDamage = selectedCharacter.baseAttackDamage;
        CurrentAttackSpeed = selectedCharacter.baseAttackSpeed;
        CurrentSpeedStat = selectedCharacter.baseSpeed;

        SpawnCharacterModel();

        // Update HUD icon to match selected character
        if (HUDSingleton.Instance != null)
        {
            HUDSingleton.Instance.SetCharacterIcon(selectedCharacter.characterIcon);
        }

        if (selectedCharacter != null)
        {
            ApplyCharacterStats(selectedCharacter);
        }
    }

    private void Update()
    {
        attackTimer += Time.deltaTime;


        // Movement
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

        // Attacking
        if (Input.GetMouseButtonDown(0) && attackTimer >= attackCooldown)
        {
            if (selectedCharacter.isMelee)
            {
                StartCoroutine(PerformMeleeAttack());
                attackTimer = 0f;
            }
            else
            {
                PerformRangedAttack();
                attackTimer = 0f;
            }
        }
    }

    /////////////////
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

        // Raycast, but ignore triggers
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, ~0, QueryTriggerInteraction.Ignore))
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

    private void UpdateSpeeds()
    {
        WalkSpeed = moveSpeed;
        SprintSpeed = moveSpeed * 1.3f; // sprint = 30% faster than walk
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

    public void SpawnCharacterModel()
    {
        if (selectedCharacter == null || selectedCharacter.characterModelPrefab == null)
        {
            Debug.LogWarning("No character data or model prefab set!");
            return;
        }

        // Clear previous model if any
        foreach (Transform child in modelPosition)
        {
            Destroy(child.gameObject);
        }

        // Spawn the new model
        GameObject model = Instantiate(selectedCharacter.characterModelPrefab, modelPosition);
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;
    }

    public void ApplyCharacterStats(CharacterData data)
    {
        selectedCharacter = data;

        maxHP = data.baseHP;
        currentHP = maxHP;
        attackDamage = data.baseAttackDamage;
        attackSpeed = Mathf.Max(data.baseAttackSpeed, 0.01f);
        moveSpeed = data.baseSpeed;

        attackCooldown = attackSpeed > 0 ? 1f / attackSpeed : 9999f;

        UpdateSpeeds();

        HUDSingleton.Instance.SetHealth(currentHP, maxHP);
        HUDSingleton.Instance.SetCharacterIcon(data.characterIcon);
    }

    private IEnumerator PerformMeleeAttack()
    {
        // Set current attack damage from character data
        int attackDamage = selectedCharacter.baseAttackDamage;

        // Set the damage on the melee hitbox script
        MeleeHitbox hitboxScript = meleeHitboxObject.GetComponent<MeleeHitbox>();
        if (hitboxScript != null)
        {
            hitboxScript.SetDamage(attackDamage);
        }

        meleeHitboxObject.SetActive(true);

        yield return new WaitForSeconds(meleeAttackDuration);

        meleeHitboxObject.SetActive(false);
    }
    private void PerformRangedAttack()
    {
        if (selectedCharacter.projectilePrefab == null || rangedFirePoint == null)
        {
            Debug.LogWarning("Projectile prefab or fire point not assigned.");
            return;
        }

            // Default direction is forward
            Vector3 direction = transform.forward;

        // Raycast to get mouse position in world space
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 target = hit.point;
            target.y = rangedFirePoint.position.y; // flatten Y to projectile height
            direction = (target - rangedFirePoint.position).normalized;
            Debug.Log("Target acquired at " + target);
        }
        else
        {
            Debug.Log("Raycast did not hit");
        }

        // Rotate the fire point to face the target direction
        rangedFirePoint.rotation = Quaternion.LookRotation(direction);

        // Instantiate projectile at fire point
        GameObject projectile = Instantiate(
            selectedCharacter.projectilePrefab,
            rangedFirePoint.position,
            rangedFirePoint.rotation
        );

        if (projectile.TryGetComponent<Projectile>(out var projScript))
        {
            projScript.Initialize(attackDamage, gameObject);
            Debug.Log("Projectile initialized and launched.");
        }
        else
        {
            Debug.LogWarning("Projectile prefab missing Projectile script.");
        }
    }

    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        HUDSingleton.Instance.SetHealth(currentHP, maxHP);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        Debug.Log("Player died!");
        // Disable movement, trigger respawn or return to lobby, etc.
    }
}
