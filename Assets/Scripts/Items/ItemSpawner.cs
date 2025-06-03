using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;
using TMPro;

public class ItemSpawner : MonoBehaviour
{
    [System.Serializable]
    public class RarityChance
    {
        public ItemData.Rarity rarity;
        [Range(0f, 100f)] public float chance; // Percent chance
    }

    [Header("Spawn Settings")]
    public List<ItemData> itemPool; // All possible items to spawn
    public List<RarityChance> rarityChances; // Chance per rarity
    public GameObject pickupPrefab;

    private GameObject spawnedPickup;

    public Transform modelSpawnPoint; // Where to spawn the item model
    public GameObject worldspaceUIPrefab; // The UI prefab to show name/desc/etc

    private GameObject spawnedModel;
    private GameObject worldspaceUI;
    private bool playerNearby = false;

    public ItemData spawnedItemData;

    private void Start()
    {
        SpawnItem();
    }

    void SpawnItem()
    {
        if (itemPool == null || itemPool.Count == 0) return;

        // Pick rarity using your logic (implement PickRarity accordingly)
        ItemData.Rarity chosenRarity = PickRarity();

        // Filter items by rarity
        List<ItemData> filteredItems = itemPool.FindAll(item => item.rarity == chosenRarity);
        if (filteredItems.Count == 0)
        {
            Debug.LogWarning($"No items of rarity {chosenRarity} in pool.");
            return;
        }

        // Select random item of chosen rarity
        ItemData selectedItem = filteredItems[Random.Range(0, filteredItems.Count)];

        // Instantiate the pickup prefab
        spawnedPickup = Instantiate(pickupPrefab, transform.position, Quaternion.identity, transform); ;

        // Assign the ItemData to the ItemPickup script on the prefab
        ItemPickup itemPickup = spawnedPickup.GetComponent<ItemPickup>();
        if (itemPickup != null)
        {
            itemPickup.SetItemData(selectedItem);
            spawnedItemData = selectedItem;

            // If you have a model spawn point on the pickup prefab:
            Transform modelSpawnPoint = spawnedPickup.transform.Find("ModelPosition");
            if (modelSpawnPoint != null && selectedItem.modelPrefab != null)
            {
                Instantiate(selectedItem.modelPrefab, modelSpawnPoint.position, Quaternion.identity, modelSpawnPoint);
            }
        }
        else
        {
            Debug.LogError("Pickup prefab missing ItemPickup component.");
        }
    }
    ItemData.Rarity PickRarity()
    {
        float totalWeight = 0f;
        foreach (var rc in rarityChances)
            totalWeight += rc.chance;

        float rand = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var rc in rarityChances)
        {
            cumulative += rc.chance;
            if (rand <= cumulative)
                return rc.rarity;
        }

        // fallback
        return ItemData.Rarity.Common;
    }

    private void Update()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            TryPickupItem();
        }
    }

    private void TryPickupItem()
    {
        var inventory = PlayerInventory.Instance;
        Debug.Log($"PlayerInventory.Instance: {PlayerInventory.Instance}");
        Debug.Log($"Found inventory with FindObjectOfType: {FindObjectOfType<PlayerInventory>()}");
        Debug.Log($"spawnedItemData: {spawnedItemData?.itemName ?? "NULL"}");

        if (inventory != null && spawnedItemData != null)
        {
            inventory.AddItem(spawnedItemData);
        }
        else
        {
            Debug.LogWarning("No PlayerInventory found or spawnedItemData is null!");
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            if (worldspaceUI != null)
                worldspaceUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            if (worldspaceUI != null)
                worldspaceUI.SetActive(false);
        }
    }
}