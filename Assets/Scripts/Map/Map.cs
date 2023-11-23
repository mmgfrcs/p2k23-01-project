using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using Utilities;

/// <summary>
/// Handles the Map coordinates and the actual spawning and tracking of enemies.
/// </summary>
public class Map : MonoBehaviour
{
    [SerializeField] private Vector2Int startPosition;
    [SerializeField] private SpawnConfig spawnConfig;
    [SerializeField] private ExpansionTiming[] expansionTimings;
    [SerializeField] private List<Vector2Int> checkpoints;
    [SerializeField] private List<Vector2Int> towerSpots;

    private Dictionary<EnemyType, ObjectPool<Enemy>> _enemyPools = new();
    private Dictionary<Vector2Int, Tower> towerDict = new();
    private Camera mainCam;
    private Grid _grid;
    
    // Start is called before the first frame update
    private void Start()
    {
        _grid = GetComponent<Grid>();
        mainCam = Camera.main;
        for (int i = 0; i < spawnConfig.spawnTimings.Count; i++)
        {
            var idx = i;
            _enemyPools.TryAdd(spawnConfig.spawnTimings[i].enemyPrefab.Type, new ObjectPool<Enemy>(
                () => Instantiate(spawnConfig.spawnTimings[idx].enemyPrefab.gameObject, Vector3.zero, Quaternion.identity).GetComponent<Enemy>(), 
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

    private void Update()
    {
        if (Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            var loc = mainCam.ScreenToWorldPoint(Input.mousePosition);
            var cellLoc = _grid.WorldToCell(loc).ToVector2Int();

            if (towerSpots.Contains(cellLoc))
            {
                if (towerDict.ContainsKey(cellLoc))
                {
                    Debug.Log("Contains tower");
                }
                else
                {
                    Debug.Log("Does not contain tower");
                }
            }
        }
    }

    /// <summary>
    /// Get the details of the enemy to spawn in a certain <paramref name="wave"/>.
    /// </summary>
    /// <param name="wave">The wave number. Must be greater than 0.</param>
    /// <returns>The enemy to spawn on that <paramref name="wave"/>, along with its amount and spawn delay</returns>
    public SpawnTiming GetSpawnTiming(uint wave)
    {
        if (wave > spawnConfig.spawnTimings.Count) return spawnConfig.spawnTimings[^1];
        if (wave == 0) { //Note that this is not possible and is a bug
            Debug.LogWarning("GetSpawnTiming called with wave 0. This is not possible!");
            return spawnConfig.spawnTimings[0]; 
        }
        
        return spawnConfig.spawnTimings[(int) (wave - 1)];
    }

    public IEnumerator SpawnEnemy(uint wave, System.Action<Enemy> onSpawn)
    {
        if (wave == 0) wave = 1;
        var st = GetSpawnTiming(wave);
        var amount = st.amount;
        while (amount > 0)
        {
            var enemy = _enemyPools[st.enemyPrefab.Type].Get();
            enemy.transform.position = _grid.GetCellCenterWorld(startPosition.ToVector3Int());
            enemy.Initialize(_grid, checkpoints.ToArray(), st.health, Mathf.Ceil((st.health-wave)/32));
            enemy.transform.localScale = Vector3.zero;
            enemy.transform.DOScale(1f, 0.6f).SetEase(Ease.OutCubic);
            enemy.JourneyComplete += EnemyOnJourneyComplete;
            amount--;
            onSpawn?.Invoke(enemy);

            yield return new WaitForSeconds(st.delay);
        }
    }

    private void EnemyOnJourneyComplete(Enemy obj)
    {
        obj.JourneyComplete -= EnemyOnJourneyComplete;
        _enemyPools[obj.Type].Release(obj);
    }
}
