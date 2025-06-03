using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/CharacterData")]
public class CharacterData : ScriptableObject
{
    
    public string characterName;
    public Sprite characterIcon;

    [Header("Stats")]
    public int baseHP;
    public int baseAttackDamage;
    public float baseAttackSpeed;
    public float baseSpeed;

    [Header("Character Type")]
    public bool isMelee;

    [Header("References")]
    public GameObject characterModelPrefab;
    [Space]
    public GameObject projectilePrefab; // only for ranged characters
}
