using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Item/Item Data")]
public class ItemData : ScriptableObject
{
    // Enums inside the ScriptableObject
    public enum StatType
    {
        AttackDamage,
        AttackSpeed,
        MovementSpeed,
        HP,
        RegenRate,
        PlayerCritDamage
    }

    public enum UniqueEffect
    {
        None,
        Poison,
        Paralysis
    }

    public enum Rarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    [System.Serializable]
    public struct StatModifier
    {
        public StatType statType;
        public bool isMultiplier; // true = multiply, false = add
        public float value;
    }

    [System.Serializable]
    public class RarityColorPair
    {
        public Rarity rarity;
        public Color color;
    }

    [Header("Basic Info")]
    public string itemName;
    [TextArea]
    public string description;
    public Rarity rarity;


    [Header("Stat Modifiers")]
    public StatModifier[] statModifiers;

    [Header("Unique Effects")]
    public UniqueEffect uniqueEffect = UniqueEffect.None;

    [Header("Visuals")]
    public GameObject modelPrefab;

    [Header("Rarity Colors")]
    public List<RarityColorPair> rarityColors;

    private Dictionary<Rarity, Color> rarityColorDict;

    private void OnEnable()
    {
        rarityColorDict = new Dictionary<Rarity, Color>();

        foreach (var pair in rarityColors)
        {
            if (!rarityColorDict.ContainsKey(pair.rarity))
            {
                rarityColorDict.Add(pair.rarity, pair.color);
            }
        }
    }

    public Color GetRarityColor()
    {
        if (rarityColorDict != null && rarityColorDict.TryGetValue(rarity, out var color))
        {
            return color;
        }
        return Color.white;
    }

    public string GetRarityHex()
    {
        Color color = GetRarityColor();
        return $"#{ColorUtility.ToHtmlStringRGB(color)}";
    }
}
