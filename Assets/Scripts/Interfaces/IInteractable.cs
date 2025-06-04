using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    void Interact();
    void OnFocus();   // Triggered when player enters range
    void OnUnfocus(); // Triggered when player exits range
}
