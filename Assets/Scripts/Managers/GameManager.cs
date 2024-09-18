using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AdInfinitum.Entities;
using AdInfinitum.UI;
using AdInfinitum.Utilities;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

namespace AdInfinitum.Managers
{
    public class GameManager : Singleton<GameManager>
    {
        [Header("Configuration"), SerializeField] private uint startingLife = 10;
        [SerializeField] private ulong startingMoney = 150;
        [SerializeField] private MapManager[] maps;
        [SerializeField] private Tower[] towers;
        [SerializeField] private float gameOverDelay = 1f;
        [SerializeField] private VFXReturner cannonExplosion, missileExplosion;

        [Header("UI"), SerializeField] private CanvasGroup mainUI;
        [SerializeField] private GameOverPanel gameOverPanel;

        private Dictionary<TowerType, ObjectPool<GameObject>> explosionPoolDict;

        public ulong Score { get; private set; }
        public ulong Highscore { get; private set; }
        public ulong Money { get; private set; }
        public uint Life { get; private set; }
        public uint Wave { get; private set; }
        public int EnemyAmount => _enemyList.Count;
        public float WaveTimer { get; private set; } = -1;
        public float EarlyWaveMoneyBonus => Mathf.Floor(Mathf.Max(WaveTimer / 2f, 0));

        // Stats
        public float DPS { get; private set; }

        public ulong Kills { get; private set; }

        public IReadOnlyList<MapManager> MapList => maps;
        public IReadOnlyList<Tower> TowerList => towers;
        public IReadOnlyList<Enemy> EnemyList => _enemyList;
        public MapManager CurrentMap => _map;

        private List<Enemy> _enemyList = new();

        private MapManager _map;
        private Coroutine _enemyCo, _dpsCo;
        private UnityEngine.Camera _sceneCamera;
        private Vector3 _mousePosInitial = Vector3.zero;
        private bool _isGameOver;

        public event Action<float> GameOver;

        private Func<GameObject> CreateExplosion(GameObject obj)
        {
            return () =>
            {
                GameObject o = Instantiate(obj);
                o.SetActive(false);
                return o;
            };
        }

        private void GetExplosion(GameObject o)
        {
            o.SetActive(true);
            o.GetComponent<Animator>().Play("Explosion");
        }
        
        
        // Start is called before the first frame update
        private void Start()
        {
            Life = startingLife;
            Money = startingMoney;
            Score = 0;
            Wave = 0;
            Kills = 0;
            Highscore = ulong.Parse(PlayerPrefs.GetString("Highscore", "0"));

            Enemy.Death += EnemyOnDeath;
            Enemy.ReachedBase += EnemyOnReachedBase;

            _sceneCamera = UnityEngine.Camera.main;

            _map = Instantiate(maps[Random.Range(0, maps.Length)].gameObject, Vector3.zero, Quaternion.identity).GetComponent<MapManager>();
            explosionPoolDict = new Dictionary<TowerType, ObjectPool<GameObject>>()
            {
                { 
                    TowerType.Cannon, new ObjectPool<GameObject>(
                        CreateExplosion(cannonExplosion.gameObject), 
                        GetExplosion, 
                        o => o.SetActive(false), 
                        Destroy) 
                },
                { 
                    TowerType.Missile, new ObjectPool<GameObject>(
                        CreateExplosion(missileExplosion.gameObject), 
                        GetExplosion, 
                        o => o.SetActive(false), 
                        Destroy) 
                },
            };
        }

        private void EnemyOnDeath(Enemy e)
        {
            Score += Convert.ToUInt64(Mathf.CeilToInt(e.MaxHealth));
            Money += Convert.ToUInt64(Mathf.CeilToInt(e.Bounty));
            Kills++;
            _enemyList.Remove(e);
        }

        private void EnemyOnReachedBase(Enemy e, uint lifecost)
        {
            Life -= lifecost;
            _enemyList.Remove(e);
        }

        // Update is called once per frame
        private void Update()
        {
            if (_isGameOver) return;
            if (Life <= 0)
            {
                //Game Over
                GameOver?.Invoke(gameOverDelay);
                mainUI.interactable = false;
                mainUI.blocksRaycasts = false;
                mainUI.DOFade(0f, gameOverDelay).SetUpdate(true);
                DOTween.To(() => Time.timeScale, value => Time.timeScale = value, 0.1f, gameOverDelay)
                    .SetUpdate(true).onComplete += () =>
                {
                    Time.timeScale = 1;
                    gameOverPanel.ShowPanel(Score, Wave, Kills, DPS);
                    if (Score > Highscore) PlayerPrefs.SetString("Highscore", Score.ToString());
                };
                _isGameOver = true;
                return;
            }

            if (WaveTimer > 0) WaveTimer -= Time.deltaTime;
            else if (WaveTimer > -1) NextWave();

            if (Input.GetMouseButtonDown(1))
                _mousePosInitial = _sceneCamera.ScreenToWorldPoint(Input.mousePosition);

            if (Input.GetMouseButton(1))
            {
                var dir = _mousePosInitial - _sceneCamera.ScreenToWorldPoint(Input.mousePosition);

                _sceneCamera.transform.position += dir;
            }

            _enemyList.Sort();
        }

        public void SpawnExplosion(TowerType type, Vector3 loc)
        {
            if (type != TowerType.Cannon && type != TowerType.Missile) return;
            var expObj = explosionPoolDict[type].Get();
            expObj.transform.position = loc;
            expObj.GetComponent<VFXReturner>().SetTargetType(type);
        }

        public void ReleaseExplosion(TowerType type, GameObject expObj)
        {
            explosionPoolDict[type].Release(expObj);
        }

        private IEnumerator StartEnemySpawn()
        {
            WaveTimer = -2;
            yield return _map.SpawnEnemy(Wave, (e) => _enemyList.Add(e));

            if (_map.ExpandThisWave(Wave + 1)) WaveTimer = -3;
            else WaveTimer = _map.GetSpawnTiming(Wave).amount * _map.GetSpawnTiming(Wave).waitTimeMultiplier;
            _enemyCo = null;
        }

        /// <summary>
        /// Get tower DPS every second. Runs indefinitely after the start of the first wave.
        /// </summary>
        /// <returns></returns>
        private IEnumerator GetDPS()
        {
            while (true)
            {
                DPS = CurrentMap.SpawnedTowerList.Count != 0
                    ? CurrentMap.SpawnedTowerList
                        .Sum(x => x.Value.Reports.DPS)
                    : 0;
                
                yield return new WaitForSeconds(1.0f);
            }
            
        }

        public void NextWave()
        {
            if (Math.Abs(WaveTimer - (-2)) < 0.001f) return; // Wave timer is not -2 (expansion incoming wave)
            if (_enemyCo != null) StopCoroutine(_enemyCo);
            _dpsCo ??= StartCoroutine(GetDPS());
            
            Money += (ulong)Mathf.RoundToInt(EarlyWaveMoneyBonus);
            Wave++;
            if (_map.ExpandThisWave(Wave))
                _map.ExpandMap(Wave);

            _enemyCo = StartCoroutine(StartEnemySpawn());
        }

        /// <summary>
        /// Purchase something with money
        /// </summary>
        /// <param name="amount">The price of the purchase</param>
        /// <returns>Whether the player has enough money for the transaction. If false, the transaction is deemed aborted.</returns>
        public bool Purchase(ulong amount)
        {
            if (Money < amount) return false;
            Money -= amount;
            return true;
        }

        /// <summary>
        /// Add money to the player
        /// </summary>
        /// <param name="amount">The amount to add</param>
        /// <remarks>Money addition has no limit. Make sure the money added is within ulong limits.</remarks>
        public void AddMoney(ulong amount)
        {
            Money += amount;
        }
    }
}
