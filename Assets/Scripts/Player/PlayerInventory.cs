using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

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

        // TODO: Apply item effects/stats to player here or in a separate system
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
        Debug.Log("Inventory cleared.");
    }
}
