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
            buttonObj.transform.Find("Icon").GetComponent<Image>().sprite = dungeon.dungeonIcon;
            buttonObj.transform.Find("Name").GetComponent<Text>().text = dungeon.dungeonDisplayName;

            int highFloor = int.TryParse(dungeon.dungeonID, out int id)
            ? SaveSystem.GetHighestFloor(id)
            : 0;
            buttonObj.transform.Find("HighScore").GetComponent<Text>().text = $"Floor {highFloor}";

            buttonObj.GetComponent<Button>().onClick.AddListener(() =>
            {
                dungeonManager.GenerateDungeonLayout(dungeon.dungeonID, 1);
            });
        }
    }

    public void CloseMenu()
    {
        gameObject.SetActive(false);
    }
}
