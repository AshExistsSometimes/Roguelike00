using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelectToggle : MonoBehaviour
{
    [SerializeField] private GameObject characterSelectUI;

    private void Update()
    {
        if (characterSelectUI == null) return;

        // Toggle on/off with Tab
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            characterSelectUI.SetActive(!characterSelectUI.activeSelf);
        }

        // Close with Escape if it's currently open
        if (Input.GetKeyDown(KeyCode.Escape) && characterSelectUI.activeSelf)
        {
            characterSelectUI.SetActive(false);
        }
    }
}

