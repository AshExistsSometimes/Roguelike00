using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum EnemyType { Melee, Ranged }

[CreateAssetMenu(menuName = "Enemy/EnemyData")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    [Header("Behavior")]
    public EnemyType enemyType;
    public float preferredRange;// Ranged enemies only

    [Header("Stats")]
    public int maxHP;
    public int damage;
    public float moveSpeed;

    [Header("References")]
    public GameObject enemyModelPrefab;
}
