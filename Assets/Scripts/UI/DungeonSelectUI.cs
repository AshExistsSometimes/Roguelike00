using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DungeonSelectUI : MonoBehaviour
{
    public GameObject dungeonButtonPrefab;
    public Transform contentParent;
    public List<DungeonData> allDungeons;

    public DungeonManager dungeonManager;

    private void Awake()
    {
        dungeonManager = DungeonManager.Instance;
        if (dungeonManager == null)
        {
            Debug.LogError("[DungeonSelectionUI] DungeonManager singleton instance not found!");
        }
    }

    private void OnEnable()
    {
        PopulateDungeonList();
        LockPlayerMovement(true);
    }

    private void OnDisable()
    {
        LockPlayerMovement(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseMenu();
        }
    }

    void LockPlayerMovement(bool locked)
    {
        // Figure out how to lock player movement later
    }

    void PopulateDungeonList()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        foreach (DungeonData dungeon in allDungeons)
        {
            GameObject buttonObj = Instantiate(dungeonButtonPrefab, contentParent);

            DungeonButtonUI buttonUI = buttonObj.GetComponent<DungeonButtonUI>();
            if (buttonUI != null)
            {
                buttonUI.Setup(dungeon);
            }
            else
            {
                Debug.LogError("DungeonButtonUI component missing from prefab!");
            }
        }
    }

    public void CloseMenu()
    {
        gameObject.SetActive(false);
    }
}
