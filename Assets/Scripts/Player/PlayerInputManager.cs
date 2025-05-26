using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputManager : MonoBehaviour
{
    public static PlayerInputManager Instance { get; private set; }

    [Header("Key Bindings")]
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode interactKey = KeyCode.E;

    // Key States (Read Only)
    public bool IsSprinting { get; private set; }
    public bool IsInteracting { get; private set; }

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        IsSprinting = Input.GetKey(sprintKey);
        IsInteracting = Input.GetKeyDown(interactKey);
    }
}