using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabletInteraction : Interactable
{
    public enum InteractionType { RaiseTablet, OpenDungeonMenu }


    [Header("Interaction")]
    public InteractionType interactionType;

    [Header("UI Reference")]
    public GameObject dungeonSelectUI;

    [Header("Raise Settings")]
    public Vector3 moveOffset = new Vector3(0f, 10f, 0f);
    public float moveDuration = 2f;

    [Header("Target To Move (Root Tablet Object)")]
    public Transform objectToMove;

    private Vector3 initialPosition;
    private bool hasActivated = false;

    private void Start()
    {
        initialPosition = objectToMove.position;

        if (SaveSystem.HasTabletBeenRaised())
        {
            // Start already raised
            objectToMove.transform.position = initialPosition + moveOffset;
            hasActivated = true;
        }
        else
        {
            // Start hidden (default)
            objectToMove.transform.position = initialPosition;
            hasActivated = false;
        }
    }

    public override void Interact()
    {
        Debug.Log("TabletInteraction Interact called!");
        base.Interact();

        switch (interactionType)
        {
            case InteractionType.RaiseTablet:
                if (hasActivated) return;
                RaiseTablet();
                break;
            case InteractionType.OpenDungeonMenu:
                OpenDungeonSelect();
                break;
        }




        hasActivated = true;
    }

    private void RaiseTablet()
    {
        if (objectToMove == null)
        {
            Debug.LogWarning("TabletInteraction: No objectToMove assigned.");
            return;
        }

        Vector3 targetPosition = objectToMove.position + moveOffset;
        StartCoroutine(MoveTablet(objectToMove.position, targetPosition, moveDuration));

        // Save to file that tablet has been raised (optional, handled via SaveSystem later)
        SaveSystem.MarkTabletRaised();
    }

    private void OpenDungeonSelect()
    {
        if (dungeonSelectUI != null)
        {
            dungeonSelectUI.SetActive(true);
        }
        else
        {
            Debug.LogWarning("No dungeonSelectUI assigned in TabletInteraction.");
        }
    }

    private IEnumerator MoveTablet(Vector3 start, Vector3 end, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            objectToMove.position = Vector3.Lerp(start, end, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        objectToMove.position = end;
    }
}

