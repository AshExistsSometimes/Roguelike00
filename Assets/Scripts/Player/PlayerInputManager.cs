using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputManager : MonoBehaviour
{
    public static PlayerInputManager Instance { get; private set; }

    [Header("Key Bindings")]
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode interactKey = KeyCode.E;
    public KeyCode attackKey = KeyCode.Mouse0;
    public KeyCode abilityMenuKey = KeyCode.Tab;
    public KeyCode pauseMenuKey = KeyCode.Escape;

    // Key States (Read Only)
    public bool SprintKey { get; private set; }//     Sprint
    public bool InteractKeyDown { get; private set; }//   Interact
    public bool AttackKeyDown { get; private set; }//     Attack
    public bool AbilitiesKeyDown { get; private set; }//  Abilities Menu
    public bool PauseKeyDown { get; private set; }//      Pause Menu

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
        SprintKey = Input.GetKey(sprintKey);
        InteractKeyDown = Input.GetKeyDown(interactKey);
        AttackKeyDown = Input.GetKeyDown(attackKey);
        AbilitiesKeyDown = Input.GetKeyDown(abilityMenuKey);
        PauseKeyDown = Input.GetKeyDown(pauseMenuKey);
    }
}