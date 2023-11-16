using System.Collections;
using System.Collections.Generic;
using Entities;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Handles the Map coordinates and the actual spawning and tracking of enemies.
/// </summary>
public class Map : MonoBehaviour
{
    [SerializeField] private Vector2Int startPosition;
    [SerializeField] private SpawnTiming[] spawnTimings;

    private Dictionary<string, ObjectPool<Enemy>> enemyPools;

    private Grid _grid;
    
    // Start is called before the first frame update
    private void Start()
    {
        _grid = GetComponent<Grid>();
        for (int i = 0; i < spawnTimings.Length; i++)
        {
            var idx = i;
            enemyPools.TryAdd(spawnTimings[i].enemyPrefab.Name, new ObjectPool<Enemy>(() =>
                {
                    Instantiate(spawnTimings[idx].enemyPrefab.gameObject, Vector3.zero, Quaternion.identity);
                    return spawnTimings[idx].enemyPrefab;
                }, enemy =>
                {
                    enemy.gameObject.SetActive(true);
                }, enemy =>
                {
                    enemy.gameObject.SetActive(false);
                }, enemy =>
                {
                    Destroy(enemy.gameObject);
                })
            );
        }
    }

    /// <summary>
    /// Get the details of the enemy to spawn in a certain <paramref name="wave"/>.
    /// </summary>
    /// <param name="wave">The wave number. Must be greater than 0.</param>
    /// <returns>The enemy to spawn on that <paramref name="wave"/>, along with its amount and spawn delay</returns>
    public SpawnTiming GetSpawnTiming(uint wave)
    {
        if (wave > spawnTimings.Length) return spawnTimings[^1];
        if (wave == 0) { //Note that this is not possible and is a bug
            Debug.LogWarning("GetSpawnTiming called with wave 0. This is not possible!");
            return spawnTimings[0]; 
        }
        
        return spawnTimings[wave - 1];
    }

    public void SpawnEnemy(uint wave)
    {
        var st = GetSpawnTiming(wave);
        var enemy = enemyPools[st.enemyPrefab.Name].Get();
        enemy.transform.position = _grid.CellToWorld(new Vector3Int(startPosition.x, startPosition.y, 0));
        enemy.Initialize(_grid, st.health, (st.health-wave)/64);
    }
}
