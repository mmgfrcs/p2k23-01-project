using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using UnityEngine.Serialization;

/// <summary>
/// Handles the Map coordinates and the actual spawning and tracking of enemies.
/// </summary>
public class Map : MonoBehaviour
{
    [SerializeField] private Vector2Int startPosition;
    [SerializeField] private MapConfig mapConfig;
    [SerializeField] private ExpansionConfig expansionConfig;
    [SerializeField] private List<Vector2Int> checkpoints;
    [SerializeField] private List<Vector2Int> towerSpots;
    [SerializeField] private Sprite selectionBox;

    private Dictionary<EnemyType, ObjectPool<Enemy>> _enemyPools = new();
    private Dictionary<Vector2Int, Tower> towerDict = new();
    private Camera mainCam;
    private Grid _grid;
    private BuyPanel buyPanel;
    private TowerDetailPanel detailPanel;

    private GameObject sBox;
    //private DetailPanel detailPanel;
    
    // Start is called before the first frame update
    private void Awake()
    {
        _grid = GetComponent<Grid>();
        mainCam = Camera.main;
        buyPanel = FindObjectOfType<BuyPanel>(true);
        detailPanel = FindObjectOfType<TowerDetailPanel>(true);
    }

    private void Start()
    {
        sBox = new GameObject("SelectionBox");
        var sr = sBox.AddComponent<SpriteRenderer>();
        sr.sprite = selectionBox;
        sr.sortingOrder = 999;
        sBox.transform.localScale = Vector3.one * 1.2f;
        sBox.gameObject.SetActive(false);
        
        for (int i = 0; i < mapConfig.spawnTimings.Count; i++)
        {
            var idx = i;
            _enemyPools.TryAdd(mapConfig.spawnTimings[i].enemyPrefab.Type, new ObjectPool<Enemy>(
                () => Instantiate(mapConfig.spawnTimings[idx].enemyPrefab.gameObject, Vector3.zero, Quaternion.identity).GetComponent<Enemy>(), 
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
        
        buyPanel.TowerBuy += BuyPanelOnTowerBuy;
    }

    private void BuyPanelOnTowerBuy(Vector3 arg1, Tower arg2)
    {
        towerDict.Add(_grid.WorldToCell(arg1).ToVector2Int(), arg2);
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            var loc = mainCam.ScreenToWorldPoint(Input.mousePosition);
            var cellLoc = _grid.WorldToCell(loc).ToVector2Int();

            if (towerSpots.Contains(cellLoc))
            {
                sBox.transform.position = _grid.GetCellCenterWorld(cellLoc.ToVector3Int());
                sBox.gameObject.SetActive(true);
                if (towerDict.TryGetValue(cellLoc, out var value))
                    detailPanel.OpenPanel(value);
                else
                    buyPanel.OpenPanel(sBox.transform.position);
            }
            else sBox.gameObject.SetActive(false);
        }
        else if (Input.GetMouseButtonDown(0)) sBox.gameObject.SetActive(false);
    }

    /// <summary>
    /// Get the details of the enemy to spawn in a certain <paramref name="wave"/>.
    /// </summary>
    /// <param name="wave">The wave number. Must be greater than 0.</param>
    /// <returns>The enemy to spawn on that <paramref name="wave"/>, along with its amount and spawn delay</returns>
    public SpawnTiming GetSpawnTiming(uint wave)
    {
        if (wave > mapConfig.spawnTimings.Count) return mapConfig.spawnTimings[^1];
        if (wave == 0) { //Note that this is not possible and is a bug
            Debug.LogWarning("GetSpawnTiming called with wave 0. This is not possible!");
            return mapConfig.spawnTimings[0]; 
        }
        
        return mapConfig.spawnTimings[(int) (wave - 1)];
    }

    public bool ExpandThisWave(uint wave)
    {
        return expansionConfig.expTimingDict.ContainsKey(wave);
    }

    public void ExpandMap(uint wave)
    {
        if (!ExpandThisWave(wave)) return;
        expansionConfig.expTimingDict[wave].expandSection.SetActive(true);
        checkpoints.AddRange(expansionConfig.expTimingDict[wave].newCheckpoints);
        towerSpots.AddRange(expansionConfig.expTimingDict[wave].newTowerSpots);
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
