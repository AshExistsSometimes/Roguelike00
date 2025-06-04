using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class SaveSystem
{
    private static readonly string savePath = Path.Combine(Application.persistentDataPath, "SaveData.txt");
    private static Dictionary<string, string> saveData = new Dictionary<string, string>();

    static SaveSystem()
    {
        Debug.Log($"[SaveSystem] Save file path: {savePath}");
        LoadSaveFile();
    }

    private static void LoadSaveFile()
    {
        saveData.Clear();

        if (!File.Exists(savePath)) return;

        string[] lines = File.ReadAllLines(savePath);
        foreach (string line in lines)
        {
            if (!string.IsNullOrWhiteSpace(line) && line.Contains("="))
            {
                string[] parts = line.Split('=');
                if (parts.Length == 2)
                {
                    saveData[parts[0].Trim()] = parts[1].Trim();
                }
            }
        }
    }

    private static void SaveToFile()
    {
        List<string> lines = new List<string>();
        foreach (var kvp in saveData)
        {
            lines.Add($"{kvp.Key}={kvp.Value}");
        }
        File.WriteAllLines(savePath, lines);
    }

    public static void ResetSaveData()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("[SaveSystem] Save file deleted.");
        }
        else
        {
            Debug.Log("[SaveSystem] No save file found to delete.");
        }

        saveData.Clear(); 
    }

    // ===== Tablet Raised =====

    public static bool HasTabletBeenRaised()
    {
        return saveData.ContainsKey("TabletRaised") && saveData["TabletRaised"].ToLower() == "true";
    }

    public static void MarkTabletRaised()
    {
        saveData["TabletRaised"] = "true";
        SaveToFile();
    }

    // ===== Dungeon High Floor Tracking =====

    public static int GetHighestFloor(int dungeonID)
    {
        string key = $"Dungeon_{dungeonID}_HighFloor";
        return saveData.ContainsKey(key) && int.TryParse(saveData[key], out int result) ? result : 0;
    }

    public static void SaveHighestFloor(int dungeonID, int floor)
    {
        string key = $"Dungeon_{dungeonID}_HighFloor";
        int currentHigh = GetHighestFloor(dungeonID);

        if (floor > currentHigh)
        {
            saveData[key] = floor.ToString();
            SaveToFile();
            Debug.Log($"New high floor for dungeon {dungeonID}: {floor}");
        }
    }
}

[System.Serializable]
public class SaveData
{
    public List<DungeonFloorRecord> floorRecords = new List<DungeonFloorRecord>();
    public bool hasRaisedTablet = false;

    // Utility: Convert to dictionary for runtime use
    public Dictionary<int, int> ToDictionary()
    {
        Dictionary<int, int> dict = new Dictionary<int, int>();
        foreach (var record in floorRecords)
        {
            dict[record.dungeonID] = record.highestFloor;
        }
        return dict;
    }

    // Utility: Update or add a floor record
    public void SetHighestFloor(int dungeonID, int floor)
    {
        var existing = floorRecords.Find(r => r.dungeonID == dungeonID);
        if (existing != null)
            existing.highestFloor = floor;
        else
            floorRecords.Add(new DungeonFloorRecord { dungeonID = dungeonID, highestFloor = floor });
    }

    // Utility: Get highest floor for a dungeon
    public int GetHighestFloor(int dungeonID)
    {
        var record = floorRecords.Find(r => r.dungeonID == dungeonID);
        return record != null ? record.highestFloor : 0;
    }
}

[System.Serializable]
public class DungeonFloorRecord
{
    public int dungeonID;
    public int highestFloor;
}