using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerAndCamCheck : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject cameraPrefab;
    public GameObject uiPrefab;
    public GameObject inputManagerPrefab;

    private void Start()
    {
        // Spawn player singleton if it doesn't exist
        if (PlayerSingleton.Instance == null)
        {
            Instantiate(playerPrefab);
        }

        // Spawn camera singleton if it doesn't exist
        if (CameraSingleton.Instance == null)
        {
            Instantiate(cameraPrefab);
        }

        // Spawn UI singleton if it doesn't exist
        if (HUDSingleton.Instance == null)
        {
            Instantiate(uiPrefab);
        }

        // Spawn Input Manager singleton if it doesn't exist
        if (PlayerInputManager.Instance == null)
        {
            Instantiate(inputManagerPrefab);
        }
    }
}
