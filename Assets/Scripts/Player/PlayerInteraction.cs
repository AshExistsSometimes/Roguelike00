using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private IInteractable currentInteractable;

    private void Update()
    {
        if (currentInteractable != null && Input.GetKeyDown(KeyCode.E))
        {
            currentInteractable.Interact();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IInteractable interactable))
        {
            currentInteractable = interactable;
            interactable.OnFocus();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out IInteractable interactable) && interactable == currentInteractable)
        {
            interactable.OnUnfocus();
            currentInteractable = null;
        }
    }
}
