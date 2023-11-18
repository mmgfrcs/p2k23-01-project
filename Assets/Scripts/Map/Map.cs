using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Handles the Map coordinates and the actual spawning and tracking of enemies.
/// </summary>
public class Map : MonoBehaviour
{
    [SerializeField] private Vector2Int startPosition;
    [SerializeField] private List<SpawnTiming> spawnTimings;
    [SerializeField] private List<Vector2Int> checkpoints;

    private Dictionary<EnemyType, ObjectPool<Enemy>> _enemyPools = new();

    private Grid _grid;
    
    // Start is called before the first frame update
    private void Start()
    {
        _grid = GetComponent<Grid>();
        for (int i = 0; i < spawnTimings.Count; i++)
        {
            var idx = i;
            _enemyPools.TryAdd(spawnTimings[i].enemyPrefab.Type, new ObjectPool<Enemy>(
                () => Instantiate(spawnTimings[idx].enemyPrefab.gameObject, Vector3.zero, Quaternion.identity).GetComponent<Enemy>(), 
                enemy =>
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
        if (wave > spawnTimings.Count) return spawnTimings[^1];
        if (wave == 0) { //Note that this is not possible and is a bug
            Debug.LogWarning("GetSpawnTiming called with wave 0. This is not possible!");
            return spawnTimings[0]; 
        }
        
        return spawnTimings[(int) (wave - 1)];
    }

    public IEnumerator SpawnEnemy(uint wave)
    {
        if (wave == 0) wave = 1;
        var st = GetSpawnTiming(wave);
        var amount = st.amount;
        while (amount > 0)
        {
            var enemy = _enemyPools[st.enemyPrefab.Type].Get();
            enemy.transform.position = _grid.GetCellCenterWorld(startPosition.ToVector3Int());
            enemy.Initialize(_grid, checkpoints.ToArray(), st.health, (st.health-wave)/64);
            enemy.JourneyComplete += EnemyOnJourneyComplete;
            amount--;

            yield return new WaitForSeconds(st.delay);
        }
    }

    private void EnemyOnJourneyComplete(Enemy obj)
    {
        _enemyPools[obj.Type].Release(obj);
    }
}
