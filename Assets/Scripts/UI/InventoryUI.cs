using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public GameObject inventoryPanel;
    public Transform itemListParent;
    public GameObject itemEntryPrefab;
    public TextMeshProUGUI statsText;

    private bool isOpen = false;

    private void Update()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "[0] Lobby") return;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            OpenInventory();
        }
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            CloseInventory();
        }
    }

    private void OpenInventory()
    {
        if (isOpen) return;
        isOpen = true;
        inventoryPanel.SetActive(true);
        PopulateInventory();
        DisplayPlayerStats();
    }

    private void CloseInventory()
    {
        isOpen = false;
        inventoryPanel.SetActive(false);
        ClearInventoryList();
    }

    private void PopulateInventory()
    {
        foreach (var item in PlayerInventory.Instance.GetCollectedItems())
        {
            GameObject entry = Instantiate(itemEntryPrefab, itemListParent);
            TextMeshProUGUI entryText = entry.GetComponentInChildren<TextMeshProUGUI>();
            string rarityHex = item.GetRarityHex();
            entryText.text = $"<color={rarityHex}>{item.itemName}</color>\n{FormatModifiers(item)}";
        }
    }

    private void ClearInventoryList()
    {
        foreach (Transform child in itemListParent)
        {
            Destroy(child.gameObject);
        }
    }

    private void DisplayPlayerStats()
    {
        // You'll replace this with your real player stat system
        PlayerMovement playerStats = FindObjectOfType<PlayerMovement>();
        if (playerStats == null)
        {
            statsText.text = "No stats found.";
            return;
        }

        statsText.text =
            $"<b>Stats</b>\n" +
            $"HP: {playerStats.CurrentMaxHP}\n" +
            $"Damage: {playerStats.CurrentDamage}\n" +
            $"Speed: {playerStats.CurrentSpeedStat}\n" +
            $"Attack Speed: {playerStats.CurrentAttackSpeed}";
    }

    private string FormatModifiers(ItemData item)
    {
        List<string> parts = new List<string>();

        foreach (var mod in item.statModifiers)
        {
            float value = mod.value;
            string sign = value > 0 ? "+" : ""; // "-" is included automatically if negative
            string color = value > 0 ? "green" : value < 0 ? "red" : "white";
            string valueText;

            if (mod.isMultiplier)
            {
                // Value is already a percentage, like 15 = +15%
                valueText = $"{sign}{value}%";
            }
            else
            {
                // Raw additive value
                valueText = $"{sign}{value}";
            }

            parts.Add($"<color={color}>{valueText} {mod.statType}</color>");
        }

        return string.Join("\n", parts);
    }
}