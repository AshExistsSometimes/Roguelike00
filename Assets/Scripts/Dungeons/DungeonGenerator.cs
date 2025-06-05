using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Runtime References")]
    public DungeonData currentDungeon;
    public int currentFloor = 1;

    private List<GameObject> spawnedRooms = new List<GameObject>();

    /// <summary>
    /// Main dungeon generation entry point.
    /// </summary>
    public void Generate(DungeonData dungeonData, int floor)
    {
        currentDungeon = dungeonData;
        currentFloor = floor;

        ClearExistingDungeon();

        int baseRoomCount = Random.Range(dungeonData.minRoomCount, dungeonData.maxRoomCount + 1);
        int totalRooms = baseRoomCount + (dungeonData.roomCountGrowthPerFloor * (floor - 1));

        Debug.Log($"[DungeonGenerator] Generating Floor {floor} with {totalRooms} intermediate rooms");

        // 1. Spawn Start Room at origin
        Vector3 spawnPosition = Vector3.zero;
        GameObject startRoom = Instantiate(dungeonData.startRoomPrefab, spawnPosition, Quaternion.identity);
        spawnedRooms.Add(startRoom);

        // 2. Spawn Intermediate Rooms
        for (int i = 0; i < totalRooms; i++)
        {
            RoomPrefabData randomRoom = GetRandomRoom(dungeonData.roomPrefabs);
            spawnPosition += new Vector3(10, 0, 0); // Linear layout, spaced out
            GameObject room = Instantiate(randomRoom.roomPrefab, spawnPosition, Quaternion.identity);
            spawnedRooms.Add(room);
        }

        // 3. Spawn End Room (Boss or Exit based on floor)
        EndRoomData endRoomData = GetEndRoomForFloor(dungeonData, floor);
        if (endRoomData == null)
        {
            Debug.LogError("[DungeonGenerator] Could not find a valid End Room to spawn!");
            return;
        }

        spawnPosition += new Vector3(10, 0, 0); // Continue in line
        GameObject endRoom = Instantiate(endRoomData.roomPrefab, spawnPosition, Quaternion.identity);
        spawnedRooms.Add(endRoom);

        Debug.Log($"[DungeonGenerator] Spawned {spawnedRooms.Count} rooms (Start + {totalRooms} + End)");
    }

    /// <summary>
    /// Removes previously generated rooms from the scene.
    /// </summary>
    private void ClearExistingDungeon()
    {
        foreach (GameObject room in spawnedRooms)
        {
            if (room != null)
                Destroy(room);
        }
        spawnedRooms.Clear();
    }

    /// <summary>
    /// Returns a random room prefab from the list.
    /// </summary>
    private RoomPrefabData GetRandomRoom(List<RoomPrefabData> rooms)
    {
        if (rooms == null || rooms.Count == 0)
        {
            Debug.LogError("[DungeonGenerator] No room prefabs assigned in DungeonData!");
            return null;
        }
        return rooms[Random.Range(0, rooms.Count)];
    }

    /// <summary>
    /// Returns an appropriate End Room for the floor (boss or exit).
    /// </summary>
    private EndRoomData GetEndRoomForFloor(DungeonData data, int floor)
    {
        EndRoomType type = (floor % data.floorsPerBoss == 0) ? EndRoomType.Boss : EndRoomType.Exit;
        List<EndRoomData> matchingRooms = data.endRoomPrefabs.FindAll(r => r.roomType == type);

        if (matchingRooms.Count == 0)
        {
            Debug.LogWarning($"[DungeonGenerator] No {type} room found in DungeonData!");
            return null;
        }

        return matchingRooms[Random.Range(0, matchingRooms.Count)];
    }
}