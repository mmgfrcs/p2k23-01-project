using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AdInfinitum.Database;
using AdInfinitum.Entities;
using AdInfinitum.UI;
using AdInfinitum.Utilities;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

namespace AdInfinitum.Managers
{
    /// <summary>
    /// Handles the Map coordinates and the actual spawning and tracking of enemies.
    /// </summary>
    public class MapManager : MonoBehaviour
    {
        [SerializeField] private Vector2Int startPosition;
        [SerializeField] private MapConfig mapConfig;
        [SerializeField] private ExpansionConfig expansionConfig;
        [SerializeField] private List<Vector2Int> checkpoints;
        [SerializeField] private List<Vector2Int> towerSpots;
        [SerializeField] private GameObject[] mapExpansions;
        [SerializeField] private GameObject[] homeBases;

        private Dictionary<EnemyType, ObjectPool<Enemy>> _enemyPools = new();
        private Dictionary<Vector2Int, Tower> _towerDict = new();
        private UnityEngine.Camera _mainCam;
        private Grid _grid;
        private TowerBuyPanel _buyPanel;
        private TowerDetailPanel _detailPanel;

        public SpawnTiming CurrentSpawnTiming { get; private set; }
        public uint EnemyRemaining { get; private set; }
        public IReadOnlyCollection<KeyValuePair<Vector2Int, Tower>> SpawnedTowerList => _towerDict;

        //private DetailPanel detailPanel;

        // Start is called before the first frame update
        private void Awake()
        {
            _grid = GetComponent<Grid>();
            _mainCam = UnityEngine.Camera.main;
            _buyPanel = FindObjectOfType<TowerBuyPanel>(true);
            _detailPanel = FindObjectOfType<TowerDetailPanel>(true);
        }

        private void Start()
        {
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

            _buyPanel.TowerBuy += BuyPanelOnTowerBuy;
            _detailPanel.TowerSell += DetailPanelOnTowerSell;

            for (int i = 0; i < mapExpansions.Length; i++) mapExpansions[i].SetActive(false);
            for (int i = 1; i < homeBases.Length; i++) homeBases[i].SetActive(false);
        }

        private void DetailPanelOnTowerSell(Vector3 arg1, Tower arg2)
        {
            _towerDict.Remove(_grid.WorldToCell(arg1).ToVector2Int());
        }

        private void BuyPanelOnTowerBuy(Vector3 arg1, Tower arg2)
        {
            _towerDict.Add(_grid.WorldToCell(arg1).ToVector2Int(), arg2);
        }

        private void Update()
        {
            if (!Input.GetMouseButtonUp(0) || EventSystem.current.IsPointerOverGameObject()) return;

            var loc = _mainCam.ScreenToWorldPoint(Input.mousePosition);
            var cellLoc = _grid.WorldToCell(loc).ToVector2Int();

            if (!towerSpots.Contains(cellLoc)) return;

            _detailPanel.ClosePanel();
            _buyPanel.ClosePanel();

            var pos = _grid.GetCellCenterWorld(cellLoc.ToVector3Int());
            if (_towerDict.TryGetValue(cellLoc, out var value))
                _detailPanel.OpenPanel(pos, value);
            else
                _buyPanel.OpenPanel(pos);
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

        public uint GetNextExpansionWave(uint wave)
        {
            return expansionConfig.expansionTimings.Min(x => x.expandOnWave <= wave ? uint.MaxValue : x.expandOnWave);
        }

        public bool ExpandThisWave(uint wave)
        {
            return expansionConfig.expTimingDict.ContainsKey(wave);
        }

        public void ExpandMap(uint wave)
        {
            if (!ExpandThisWave(wave)) return;
            var idx = expansionConfig.expansionTimings.IndexOf(expansionConfig.expTimingDict[wave]);
            mapExpansions[idx].SetActive(true);
            homeBases[idx+1].SetActive(true);
            homeBases[idx].SetActive(false);
            checkpoints.AddRange(expansionConfig.expTimingDict[wave].newCheckpoints);
            towerSpots.AddRange(expansionConfig.expTimingDict[wave].newTowerSpots);
        }

        public IEnumerator SpawnEnemy(uint wave, System.Action<Enemy> onSpawn)
        {
            if (wave == 0) wave = 1;
            CurrentSpawnTiming = GetSpawnTiming(wave);
            EnemyRemaining = CurrentSpawnTiming.amount;
            while (EnemyRemaining > 0)
            {
                var enemy = _enemyPools[CurrentSpawnTiming.enemyPrefab.Type].Get();
                var offset = new Vector3(Random.Range(-0.25f, 0.25f), Random.Range(-0.25f, 0.25f), 0);
                enemy.transform.position = _grid.GetCellCenterWorld(startPosition.ToVector3Int()) + offset;
                enemy.Initialize(_grid, checkpoints.ToArray(), offset, CurrentSpawnTiming.health, Mathf.Ceil((CurrentSpawnTiming.health-wave)/56));
                enemy.transform.localScale = Vector3.zero;
                enemy.transform.DOScale(1f, 0.6f).SetEase(Ease.OutCubic);
                enemy.JourneyComplete += EnemyOnJourneyComplete;
                EnemyRemaining--;
                onSpawn?.Invoke(enemy);

                if(EnemyRemaining > 0) yield return new WaitForSeconds(CurrentSpawnTiming.delay);
            }
        }

        private void EnemyOnJourneyComplete(Enemy obj)
        {
            obj.JourneyComplete -= EnemyOnJourneyComplete;
            _enemyPools[obj.Type].Release(obj);
        }
    }
}
