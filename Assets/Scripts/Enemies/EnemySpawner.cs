using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.ParticleSystem;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject enemyPrefab;
    public int numberToSpawn = 5;
    public bool spawnInBursts = true;
    public float waveInterval = 1.5f;

    [Header("Spawn Area")]
    public Transform[] spawnPoints;

    [Header("Events")]
    public UnityEvent onAllEnemiesDefeated;

    private List<GameObject> activeEnemies = new List<GameObject>();
    private int enemiesSpawned = 0;

    private void SpawnAllAtOnce()
    {
        for (int i = 0; i < numberToSpawn; i++)
        {
            SpawnEnemy();
        }
    }

    private IEnumerator SpawnInWaves()
    {
        for (int i = 0; i < numberToSpawn; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(waveInterval);
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab == null || spawnPoints.Length == 0) return;

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject newEnemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        activeEnemies.Add(newEnemy);

        Enemy enemyComponent = newEnemy.GetComponent<Enemy>();
        if (enemyComponent != null)
        {
            enemyComponent.OnDeath += () =>
            {
                activeEnemies.Remove(newEnemy);
                if (activeEnemies.Count == 0)
                {
                    onAllEnemiesDefeated?.Invoke();
                }
            };
        }

        enemiesSpawned++;
    }

    public void StartSpawning()
    {
        if (spawnInBursts)
        {
            SpawnAllAtOnce();
        }    
        else
        {
            StartCoroutine(SpawnInWaves());
        }
            
    }
}
