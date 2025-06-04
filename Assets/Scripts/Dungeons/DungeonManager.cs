using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DungeonManager : MonoBehaviour
{
    public static DungeonManager Instance { get; private set; }

    public List<DungeonData> allDungeons;
    public DungeonData CurrentDungeon { get; private set; }
    public int CurrentFloor { get; private set; }

    private const string SaveFileName = "SaveData.txt";
    private Dictionary<string, int> highScores = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadSaveData();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name.Contains("Lobby") || scene.name.Contains("Tutorial"))
            return;
    }

    // === Run Lifecycle ===
    public void BeginRun(DungeonData dungeon)
    {
        CurrentDungeon = dungeon;
        CurrentFloor = 1;
        SceneManager.LoadScene(CurrentDungeon.floorSceneName);
    }

    public void AdvanceFloor()
    {
        CurrentFloor++;
        SceneManager.LoadScene(CurrentDungeon.floorSceneName);
    }

    public void EndRun()
    {
        TrySaveHighScore(CurrentDungeon.dungeonID, CurrentFloor);
        ResetRun();
    }

    public void ResetRun()
    {
        CurrentDungeon = null;
        CurrentFloor = 0;
    }

    // === Dungeon Gen Call ===
    public void GenerateDungeonLayout(string dungeonID, int floor)
    {
        CurrentDungeon = allDungeons.Find(d => d.dungeonID == dungeonID);

        if (CurrentDungeon == null)
        {
            Debug.LogError($"[DungeonManager] Dungeon with ID '{dungeonID}' not found!");
            return;
        }

        CurrentFloor = floor;

        Debug.Log($"[DungeonManager] Generating dungeon: {CurrentDungeon.dungeonDisplayName} | Floor {CurrentFloor}");

        // TODO: Call procedural generator
        // ProceduralGenerator.Generate(CurrentDungeon, CurrentFloor);
    }

    // === Save/Load ===
    private void LoadSaveData()
    {
        highScores.Clear();

        string path = Path.Combine(Application.persistentDataPath, SaveFileName);
        if (!File.Exists(path)) return;

        string[] lines = File.ReadAllLines(path);
        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            string[] split = line.Split('=');
            if (split.Length == 2 && int.TryParse(split[1], out int score))
            {
                highScores[split[0]] = score;
            }
        }
    }

    private void SaveToFile()
    {
        string path = Path.Combine(Application.persistentDataPath, SaveFileName);
        using StreamWriter writer = new StreamWriter(path, false);
        foreach (var kvp in highScores)
        {
            writer.WriteLine($"{kvp.Key}={kvp.Value}");
        }
    }

    private void TrySaveHighScore(string dungeonID, int floor)
    {
        if (string.IsNullOrEmpty(dungeonID)) return;

        if (!highScores.ContainsKey(dungeonID) || floor > highScores[dungeonID])
        {
            highScores[dungeonID] = floor;
            SaveToFile();
        }
    }

    public int GetHighScore(string dungeonID)
    {
        return highScores.TryGetValue(dungeonID, out int score) ? score : 0;
    }
}
