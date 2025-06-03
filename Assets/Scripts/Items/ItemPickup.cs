using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;
using TMPro;

public class ItemPickup : MonoBehaviour
{
    public ItemData itemData;
    public float pickupRadius = 3f;

    [Header("UI Elements")]
    public GameObject worldspaceUI;
    public TMP_Text itemNameText;
    public TMP_Text descriptionText;
    public TMP_Text statsText;

    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (worldspaceUI != null)
            worldspaceUI.SetActive(false);
        UpdateUI();
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= pickupRadius)
        {
            if (worldspaceUI != null && !worldspaceUI.activeSelf)
                worldspaceUI.SetActive(true);

            if (Input.GetKeyDown(KeyCode.E))
            {
                PlayerInventory.Instance.AddItem(itemData);
                Destroy(gameObject);
            }
        }
        else
        {
            if (worldspaceUI != null && worldspaceUI.activeSelf)
                worldspaceUI.SetActive(false);
        }
    }


    public void SetItemData(ItemData data)
    {
        itemData = data;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (itemData == null) return;

        if (itemNameText != null)
        {
            itemNameText.text = $"<color={itemData.GetRarityHex()}>{itemData.itemName}</color>";
        }
        if (descriptionText != null) descriptionText.text = itemData.description;

        if (statsText != null)
        {
            statsText.text = "";
            foreach (var mod in itemData.statModifiers)
            {
                string colorTag = mod.value >= 0 ? "<color=#4CFF4C>" : "<color=#FF4C4C>"; // Green if positive, red if negative
                string sign = mod.isMultiplier ? "x" : (mod.value >= 0 ? "+" : "-");
                string valStr = mod.isMultiplier ? mod.value.ToString("F2") : Mathf.Abs(mod.value).ToString("F1");

                statsText.text += $"{mod.statType}: {colorTag}{sign}{valStr}</color>\n";
            }

            if (itemData.uniqueEffect != ItemData.UniqueEffect.None)
            {
                statsText.text += $"Effect: {itemData.uniqueEffect}\n";
            }
        }
    }
}
