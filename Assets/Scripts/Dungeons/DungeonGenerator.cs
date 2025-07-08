using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DungeonGenerator : MonoBehaviour
{
    [Header("Runtime References")]
    public DungeonData currentDungeon;
    public int currentFloor = 1;
    private HashSet<RoomInstance> criticalPathRooms = new();

    private Dictionary<RoomInstance, RoomPrefabData> roomDataLookup = new Dictionary<RoomInstance, RoomPrefabData>();
    private List<GameObject> spawnedRooms = new List<GameObject>();
    private RoomInstance startRoom;
    private RoomInstance endRoom;

    // For visualizing the dungeon graph in Gizmos
    private List<(Vector3, Vector3)> redPathEdges = new();
    private List<(Vector3, Vector3)> bluePathEdges = new();

    public void Generate(DungeonData dungeonData, int floor)
    {
        currentDungeon = dungeonData;
        currentFloor = floor;
        ClearExistingDungeon();

        int baseRoomCount = Random.Range(dungeonData.minRoomCount, dungeonData.maxRoomCount + 1);
        int totalRooms = baseRoomCount + (dungeonData.roomCountGrowthPerFloor * (floor - 1));
        Debug.Log($"[DungeonGenerator] Generating Floor {floor} with {totalRooms} intermediate rooms");

        // STEP 1: Place Start Room
        GameObject startObj = Instantiate(dungeonData.startRoomPrefab, Vector3.zero, Quaternion.Euler(0, 180, 0));
        startRoom = startObj.GetComponent<RoomInstance>();
        spawnedRooms.Add(startObj);

        List<RoomInstance> openRooms = new List<RoomInstance> { startRoom };
        int roomsPlaced = 0;

        // STEP 2: Spawn Intermediate Rooms
        bool forceCorridorNext = false;
        int corridorStreak = 0;

        while (roomsPlaced < totalRooms && openRooms.Count > 0)
        {
            RoomInstance fromRoom = openRooms[0];
            openRooms.RemoveAt(0);

            var fromDoors = fromRoom.GetUnconnectedDoors()
                .OrderByDescending(d => d.Forward.z)
                .ToList();

            bool attachedAnyRoom = false;

            foreach (var fromDoor in fromDoors)
            {
                if (!CanPlaceInFront(fromDoor)) continue;

                RoomPrefabData nextRoomData = null;

                for (int a = 0; a < 10; a++)
                {
                    var candidate = GetRandomRoom(currentDungeon.roomPrefabs);
                    if (candidate == null || candidate.doorCount < 2) continue;

                    bool allowThisOne = true;

                    if (forceCorridorNext)
                    {
                        allowThisOne = candidate.isCorridor;
                    }
                    else if (corridorStreak >= 1)
                    {
                        if (candidate.isCorridor)
                        {
                            // 25% chance to accept another corridor
                            allowThisOne = UnityEngine.Random.value < 0.25f;
                        }
                        else
                        {
                            allowThisOne = true;
                        }
                    }

                    if (allowThisOne)
                    {
                        nextRoomData = candidate;
                        break;
                    }
                }

                if (nextRoomData == null) continue;

                if (TryAttachRoom(nextRoomData, fromDoor, out RoomInstance newRoom))
                {
                    openRooms.Add(newRoom);
                    roomsPlaced++;
                    attachedAnyRoom = true;

                    // Update corridor tracking
                    if (nextRoomData.isCorridor)
                    {
                        corridorStreak++;
                        forceCorridorNext = false;
                    }
                    else
                    {
                        corridorStreak = 0;
                        forceCorridorNext = true;
                    }

                    // if fromRoom has more than 2 doors, spawn corridors for all remaining unconnected doors**
                    if (roomDataLookup.TryGetValue(fromRoom, out var fromRoomData))
                    {
                        if (!fromRoomData.isCorridor && fromRoom.Doors.Count > 1)
                        {
                            var remainingDoors = fromRoom.GetUnconnectedDoors();
                            foreach (var extraDoor in remainingDoors)
                            {
                                if (!CanPlaceInFront(extraDoor)) continue;

                                var corridorRoom = currentDungeon.roomPrefabs
                                    .Where(r => r.isCorridor)
                                    .OrderBy(_ => UnityEngine.Random.value)
                                    .FirstOrDefault();

                                if (corridorRoom != null)
                                {
                                    TryAttachRoom(corridorRoom, extraDoor, out RoomInstance corridorInstance);
                                    if (corridorInstance != null)
                                    {
                                        openRooms.Add(corridorInstance);
                                        roomDataLookup[newRoom] = nextRoomData;
                                        roomsPlaced++;

                                        if (nextRoomData.isCorridor)
                                        {
                                            corridorStreak++;
                                            forceCorridorNext = false;
                                        }
                                        else
                                        {
                                            corridorStreak = 0;
                                            forceCorridorNext = true;
                                        }

                                        break;
                                    }
                                }
                            }
                        }
                    }
                    break;
                }
            }
        }

        // STEP 3: Place End Room (Boss or Exit)
        var endRoomData = GetEndRoomForFloor(dungeonData, floor);
        bool endRoomPlaced = false;

        if (endRoomData != null)
        {
            var allDoors = FindObjectsOfType<DoorAnchor>()
                .Where(d => !d.isConnected && !d.transform.root.CompareTag("StartRoom"))
                .OrderByDescending(d => Vector3.Distance(d.Position, startRoom.transform.position))
                .ToList();

            foreach (var fromDoor in allDoors)
            {
                if (!CanPlaceInFront(fromDoor)) continue;

                // Prevent boss entrances facing +Z
                if (endRoomData.roomType == EndRoomType.Boss &&
                    Vector3.Dot(fromDoor.Forward, Vector3.forward) > 0.7f)
                {
                    continue;
                }

                if (TryAttachRoom(endRoomData, fromDoor, out RoomInstance newEndRoom))
                {
                    endRoom = newEndRoom;
                    endRoomPlaced = true;
                    Debug.Log("[DungeonGenerator] Successfully placed end room.");
                    break;
                }
            }
        }
        if (!endRoomPlaced)
        {
            Debug.LogWarning("[DungeonGenerator] Failed to place end room, retrying...");
            ClearExistingDungeon();
            Generate(dungeonData, floor);
        }

        // STEP 4: Loop Closure using only 2-door rooms
        TryCloseLoops();

        // STEP 5: Cap remaining unconnected doors with 1-door rooms
        CapUnconnectedDoors();
        CapUnconnectedDoors();// Band Aid in case it doeesnt get every single door, usually there are so few left uncapped on larger levels this should cover them all

        // STEP 6: Cleanup leftover issues
        CleanUpGeneration();

        Debug.Log($"[DungeonGenerator] Total rooms placed: {spawnedRooms.Count}");

        if (roomsPlaced < dungeonData.minRoomCount)
        {
            Debug.LogWarning($"Generated only {roomsPlaced} rooms, below min of {dungeonData.minRoomCount}. Retrying...");
            ClearExistingDungeon();
            Generate(dungeonData, floor);
        }

        if (roomsPlaced > (dungeonData.maxRoomCount + (dungeonData.maxRoomCount * 0.5)))
        {
            Debug.LogWarning($"Generated too many rooms for floor, below max of {dungeonData.maxRoomCount}. Retrying...");
            ClearExistingDungeon();
            Generate(dungeonData, floor);
        }

        // STEP 6: Build graph for Gizmo drawing
        BuildRoomConnectionGraph();

        // STEP 7: (teleport player to spawn (TO BE IMPLEMENTED AGAIN CUZ IT WAS BROKEN)
        StartCoroutine(TeleportPlayerToStartRoom());
    }

    private void CleanUpGeneration()
    {
        Debug.Log("[DungeonGenerator] Starting CleanUpGeneration...");

        // Use the criticalPathRooms set built during BuildRoomConnectionGraph
        var pathRooms = criticalPathRooms;

        // Step 1: Find all unconnected doors excluding the StartRoom
        var unconnectedDoors = FindObjectsOfType<DoorAnchor>()
            .Where(d => !d.isConnected && !d.transform.root.CompareTag("StartRoom"))
            .ToList();

        // Step 2: Collect unique RoomInstances that contain unconnected doors
        HashSet<RoomInstance> roomsToRemove = new HashSet<RoomInstance>();
        foreach (var door in unconnectedDoors)
        {
            var room = door.GetComponentInParent<RoomInstance>();
            if (room != null && room != startRoom && room != endRoom && !pathRooms.Contains(room))
            {
                roomsToRemove.Add(room);
            }
        }

        // Step 3: For each room to remove, disconnect its doors first
        foreach (var room in roomsToRemove)
        {
            foreach (var door in room.Doors)
            {
                if (door.isConnected && door.connectedTo != null)
                {
                    // Disconnect the counterpart door
                    var connectedDoor = door.connectedTo;
                    connectedDoor.isConnected = false;
                    connectedDoor.connectedTo = null;

                    // Disconnect this door too
                    door.isConnected = false;
                    door.connectedTo = null;
                }
            }
        }

        // Step 4: Remove the rooms and update tracking
        foreach (var room in roomsToRemove)
        {
            if (spawnedRooms.Contains(room.gameObject))
            {
                spawnedRooms.Remove(room.gameObject);
                DestroyImmediate(room.gameObject);
            }
        }

        // Step 5: Re-run CapUnconnectedDoors to fill gaps
        CapUnconnectedDoors();
        CapUnconnectedDoors(); // Second pass

        Debug.Log($"[DungeonGenerator] CleanUp complete. Removed {roomsToRemove.Count} rooms.");
    }


    private RoomInstance GetRoomAtPosition(Vector3 position)
    {
        foreach (var obj in spawnedRooms)
        {
            if (obj == null) continue;
            var room = obj.GetComponent<RoomInstance>();
            if (room != null && Vector3.Distance(room.transform.position, position) < 0.5f)
            {
                return room;
            }
        }
        return null;
    }

    private void TryCloseLoops()
    {
        var twoDoorRooms = currentDungeon.roomPrefabs.Where(r => r.doorCount == 2).ToList();
        if (twoDoorRooms.Count == 0)
        {
            Debug.LogWarning("[DungeonGenerator] No 2-door rooms for loop closure.");
            return;
        }

        var unconnected = FindObjectsOfType<DoorAnchor>()
            .Where(d => !d.isConnected && !d.transform.root.CompareTag("StartRoom"))
            .ToList();

        for (int i = 0; i < unconnected.Count; i++)
        {
            var a = unconnected[i];
            if (a.isConnected) continue;

            for (int j = i + 1; j < unconnected.Count; j++)
            {
                var b = unconnected[j];
                if (b.isConnected) continue;
                foreach (var roomData in twoDoorRooms.OrderBy(_ => Random.value))
                {
                    if (!CanPlaceInFront(a)) break;
                    if (TryAttachRoom(roomData, a, out RoomInstance bridge))
                    {
                        var exit = bridge.GetUnconnectedDoor();
                        if (exit != null && TryAttachRoom(roomData, exit, out _))
                        {
                            break;
                        }
                    }
                }
            }
        }
    }

    private void CapUnconnectedDoors()
    {
        var deadEnds = currentDungeon.roomPrefabs.Where(r => r.doorCount == 1).ToList();
        if (deadEnds.Count == 0)
        {
            Debug.LogWarning("[DungeonGenerator] No dead-end rooms available.");
            return;
        }

        var doors = FindObjectsOfType<DoorAnchor>()
            .Where(d => !d.isConnected && !d.transform.root.CompareTag("StartRoom"))
            .ToList();

        foreach (var door in doors)
        {
            if (!CanPlaceInFront(door)) continue;
            foreach (var roomData in deadEnds.OrderBy(_ => Random.value))
            {
                if (TryAttachRoom(roomData, door, out _))
                    break;
            }
        }
    }



    private void BuildRoomConnectionGraph()
    {
        redPathEdges.Clear();
        bluePathEdges.Clear();
        criticalPathRooms.Clear();

        var graph = new Dictionary<RoomInstance, List<RoomInstance>>();
        foreach (var obj in spawnedRooms)
        {
            if (obj == null) continue;
            var room = obj.GetComponent<RoomInstance>();
            if (!graph.ContainsKey(room)) graph[room] = new List<RoomInstance>();

            foreach (var door in room.Doors)
            {
                if (door.isConnected && door.connectedTo != null)
                {
                    var other = door.connectedTo.GetComponentInParent<RoomInstance>();
                    if (other != room)
                    {
                        if (!graph[room].Contains(other)) graph[room].Add(other);
                        if (!graph.ContainsKey(other)) graph[other] = new List<RoomInstance>();
                        if (!graph[other].Contains(room)) graph[other].Add(room);
                    }
                }
            }
        }

        // shortest path
        var visited = new HashSet<RoomInstance> { startRoom };
        var cameFrom = new Dictionary<RoomInstance, RoomInstance>();
        var queue = new Queue<RoomInstance>();
        queue.Enqueue(startRoom);

        while (queue.Count > 0)
        {
            var cur = queue.Dequeue();
            if (cur == endRoom) break;
            foreach (var nb in graph[cur])
            {
                if (!visited.Contains(nb))
                {
                    visited.Add(nb);
                    cameFrom[nb] = cur;
                    queue.Enqueue(nb);
                }
            }
        }

        // Reconstruct red path and track critical rooms
        if (endRoom != null && cameFrom.ContainsKey(endRoom))
        {
            var cur = endRoom;
            criticalPathRooms.Add(cur);

            while (cur != startRoom)
            {
                var prev = cameFrom[cur];
                redPathEdges.Add((prev.transform.position, cur.transform.position));
                criticalPathRooms.Add(prev);
                cur = prev;
            }
        }

        // collect all other edges as blue
        foreach (var kvp in graph)
        {
            foreach (var nb in kvp.Value)
            {
                var e = (kvp.Key.transform.position, nb.transform.position);
                if (!redPathEdges.Contains(e) && !redPathEdges.Contains((e.Item2, e.Item1)))
                    bluePathEdges.Add(e);
            }
        }
    }



private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        Gizmos.color = Color.red;
        foreach (var e in redPathEdges) Gizmos.DrawLine(e.Item1, e.Item2);
        Gizmos.color = Color.blue;
        foreach (var e in bluePathEdges) Gizmos.DrawLine(e.Item1, e.Item2);


        if (criticalPathRooms == null) return;

        Gizmos.color = Color.red;
        foreach (var room in criticalPathRooms)
        {
            if (room != null)
            {
                Gizmos.DrawWireCube(room.transform.position, Vector3.one * 3);
            }
        }
#endif
    }

    private void ClearExistingDungeon()
    {
        foreach (var obj in spawnedRooms)
            if (obj != null) DestroyImmediate(obj);

        spawnedRooms.Clear();
        redPathEdges.Clear();
        bluePathEdges.Clear();
    }

    private RoomPrefabData GetRandomRoom(List<RoomPrefabData> list)
    {
        if (list == null || list.Count == 0) return null;
        return list[Random.Range(0, list.Count)];
    }

    private EndRoomData GetEndRoomForFloor(DungeonData data, int floor)
    {
        if (data == null) return null;
        var type = (floor % data.floorsPerBoss == 0) ? EndRoomType.Boss : EndRoomType.Exit;
        return data.endRoomPrefabs.FirstOrDefault(r => r.roomType == type);
    }

    private bool TryAttachRoom(RoomPrefabData roomData, DoorAnchor fromDoor, out RoomInstance newRoom)
    {
        newRoom = null;
        if (roomData.roomPrefab == null) return false;

        // Spawn room invisibly for alignment
        var obj = Instantiate(roomData.roomPrefab);
        obj.SetActive(false);
        var inst = obj.GetComponent<RoomInstance>();
        if (inst == null) { DestroyImmediate(obj); return false; }

        // Find one of its unconnected doors
        var toDoor = inst.GetUnconnectedDoor();
        if (toDoor == null) { DestroyImmediate(obj); return false; }

        // Align and check overlap
        AlignDoors(toDoor, fromDoor);
        var bounds = GetRoomBounds(obj);
        if (Physics.OverlapBox(bounds.center, bounds.extents * 0.95f, obj.transform.rotation)
            .Any(hit => hit.transform.root != obj.transform))
        {
            DestroyImmediate(obj);
            return false;
        }

        // Commit room
        obj.SetActive(true);
        spawnedRooms.Add(obj);

        // Mark doors connected
        fromDoor.isConnected = true;
        fromDoor.connectedTo = toDoor;
        toDoor.isConnected = true;
        toDoor.connectedTo = fromDoor;

        newRoom = inst;
        return true;
    }

    private void AlignDoors(DoorAnchor toDoor, DoorAnchor fromDoor)
    {
        // Rotate so doors face each other
        var targetRot = Quaternion.LookRotation(-fromDoor.Forward, Vector3.up);
        var currentRot = Quaternion.LookRotation(toDoor.Forward, Vector3.up);
        var delta = targetRot * Quaternion.Inverse(currentRot);

        var root = toDoor.transform.root;
        root.rotation = delta * root.rotation;

        // Position so doors coincide
        var offset = fromDoor.Position - toDoor.Position;
        root.position += offset;
    }

    private Bounds GetRoomBounds(GameObject roomObj)
    {
        var rens = roomObj.GetComponentsInChildren<Renderer>();
        if (rens.Length == 0)
            return new Bounds(roomObj.transform.position, Vector3.one);

        var b = rens[0].bounds;
        for (int i = 1; i < rens.Length; i++) b.Encapsulate(rens[i].bounds);
        return b;
    }

    private bool CanPlaceInFront(DoorAnchor door)
    {
        var origin = door.Position + door.Forward * 0.5f;
        return !Physics.CheckSphere(origin + door.Forward * 1.0f, 0.45f);
    }

    private IEnumerator TeleportPlayerToStartRoom()
    {
        // Short delay to ensure rooms are generated
        yield return new WaitForSeconds(0.01f);
        Debug.Log("Finding Spawnpoint");
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawnPoint");
        Debug.Log($"Spawnpoint found at: {spawnPoint.transform.position}");

        if (spawnPoint != null && PlayerMovement.Instance != null)
        {
            Debug.Log("Teleporting Player..");
            PlayerMovement.Instance.TeleportTo(spawnPoint.transform.position);
            Debug.Log($"Sending Player to: {spawnPoint.transform.position}");
        }
        else
        {
            Debug.LogWarning("Could not find PlayerSpawnPoint or PlayerController.");
        }
    }

    public void GenerateNextFloor()
    {
        // Clear the existing dungeon first
        ClearExistingDungeon();

        // Increment the current floor number
        currentFloor++;

        // Generate the next floor using the same dungeon data
        Generate(currentDungeon, currentFloor);

    }

}

