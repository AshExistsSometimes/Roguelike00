using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }
    private PlayerMovement player;

    // The list of items collected this run
    public List<ItemData> collectedItems = new List<ItemData>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        player = GetComponent<PlayerMovement>();
    }

    // Adds an item to the inventory
    public void AddItem(ItemData item)
    {
        if (item == null)
        {
            Debug.LogWarning("Trying to add null item to inventory");
            return;
        }

        collectedItems.Add(item);
        Debug.Log($"Added item: {item.itemName} to inventory. Total items: {collectedItems.Count}");

        // Recalculate player stats
        if (player != null)
        {
            player.RecalculateStatsFromInventory();
        }
    }

    // Returns the current list of collected items (read-only)
    public IReadOnlyList<ItemData> GetCollectedItems()
    {
        return collectedItems.AsReadOnly();
    }

    // Clear inventory (e.g. when dungeon run ends)
    public void ClearInventory()
    {
        collectedItems.Clear();
        StartCoroutine(RecalculateWhenPlayerExists());
        Debug.Log("Inventory cleared.");
    }

    private IEnumerator RecalculateWhenPlayerExists()
    {
        PlayerMovement player = null;

        // Wait until player exists
        while (player == null)
        {
            player = FindObjectOfType<PlayerMovement>();
            yield return null;
        }

        player.RecalculateStatsFromInventory();
    }
}
