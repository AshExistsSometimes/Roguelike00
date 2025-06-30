using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextFloorInteractable : Interactable
{
    private DungeonGenerator dungeonGenerator;

    private void Awake()
    {
        dungeonGenerator = FindObjectOfType<DungeonGenerator>();
        if (dungeonGenerator == null)
        {
            Debug.LogError("No DungeonGenerator found in the scene.");
        }
    }

    public override void Interact()
    {
        if (dungeonGenerator != null && gameObject.name.Contains("Exit"))
        {
            dungeonGenerator.GenerateNextFloor();
        }
        else
        {
            Debug.LogWarning("DungeonGenerator not assigned on NextFloorInteractable.");
        }
    }
}
