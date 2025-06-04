using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interactable : MonoBehaviour, IInteractable
{
    public GameObject interactionPrompt;

    public virtual void Interact()
    {
        Debug.Log($"Default Interact called on {gameObject.name}");
    }

    public virtual void OnFocus()
    {
        if (interactionPrompt != null) interactionPrompt.SetActive(true);
    }

    public virtual void OnUnfocus()
    {
        if (interactionPrompt != null) interactionPrompt.SetActive(false);
    }
}
