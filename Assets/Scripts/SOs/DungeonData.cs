using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dungeon/Procedural Dungeon Data")]
public class DungeonData : ScriptableObject
{
    [Header("Identification")]
    public string dungeonID; // Unique ID used in save data and lobby UI
    public string dungeonDisplayName; // Display name
    public Sprite dungeonIcon;
    [Header("Scene Reference")]
    public string floorSceneName; // Scene to load for floors of this dungeon

    [Header("Room Prefabs")]
    public GameObject startRoomPrefab;

    [Tooltip("A list of rooms used between the start and end.")]
    public List<RoomPrefabData> roomPrefabs = new List<RoomPrefabData>();

    [Tooltip("Possible end room prefabs, marked as either Exit or Boss.")]
    public List<EndRoomData> endRoomPrefabs = new List<EndRoomData>();

    [Header("Room Count Scaling")]
    public int minRoomCount = 3;
    public int maxRoomCount = 5;
    public int roomCountGrowthPerFloor = 1;

    [Header("Boss Room Frequency")]
    [Tooltip("How many floors between bosses. 1 = boss every floor.")]
    public int floorsPerBoss = 3;
}

[System.Serializable]
public class RoomPrefabData
{
    public GameObject roomPrefab;

    [Tooltip("Number of doors/connectors in this room.")]
    public int doorCount;
}

[System.Serializable]
public class EndRoomData
{
    public GameObject roomPrefab;
    public EndRoomType roomType;
}

public enum EndRoomType
{
    Exit,
    Boss
}
