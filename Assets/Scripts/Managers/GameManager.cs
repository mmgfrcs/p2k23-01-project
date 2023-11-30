using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [Header("Configuration"), SerializeField] private int startingLife = 10;
    [SerializeField] private int startingMoney = 150;
    [SerializeField] private Map[] maps;
    [SerializeField] private Tower[] towers;

    public int Score { get; private set; }
    public int Money { get; private set; }
    public int Life { get; private set; }
    public uint Wave { get; private set; }
    public int EnemyAmount => enemyList.Count;
    public float WaveTimer { get; private set; } = -1;

    public IReadOnlyList<Map> MapList { get; private set; }
    public IReadOnlyList<Tower> TowerList { get; private set; }
    public IReadOnlyList<Enemy> EnemyList { get; private set; }

    private List<Enemy> enemyList = new();

    public static GameManager Instance { get; private set; }

    private Map _map;
    private Coroutine _enemyCo;
    private Camera _sceneCamera;
    private Vector3 _mousePosInitial = Vector3.zero;
    
    // Start is called before the first frame update
    private void Start()
    {
        AssignSingleton();
        Life = startingLife;
        Money = startingMoney;
        Score = 0;
        Wave = 0;
        EnemyList = enemyList;
        MapList = new List<Map>(maps);
        TowerList = new List<Tower>(towers);
        
        Enemy.Death += EnemyOnDeath;
        Enemy.ReachedBase += EnemyOnReachedBase;
        
        _sceneCamera = Camera.main;

        _map = Instantiate(maps[Random.Range(0, maps.Length)].gameObject, Vector3.zero, Quaternion.identity).GetComponent<Map>();
    }

    private void EnemyOnDeath(Enemy e)
    {
        Score += Mathf.CeilToInt(e.MaxHealth);
        Money += Mathf.CeilToInt(e.Bounty);
        enemyList.Remove(e);
    }

    private void EnemyOnReachedBase(Enemy e, int lifecost)
    {
        Life -= lifecost;
        enemyList.Remove(e);
    }

    private void OnDisable()
    {
        DeassignSingleton();
    }

    private void OnEnable()
    {
        AssignSingleton();
    }

    // Update is called once per frame
    private void Update()
    {
        if (WaveTimer > 0) WaveTimer -= Time.deltaTime;
        else if (WaveTimer > -1) NextWave();
        
        if (Input.GetMouseButtonDown(1))
            _mousePosInitial = _sceneCamera.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButton(1))
        {
            var dir = _mousePosInitial - _sceneCamera.ScreenToWorldPoint(Input.mousePosition);

            _sceneCamera.transform.position += dir;
        }
        
        enemyList.Sort();
    }

    private void AssignSingleton()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(this);
    }
    
    private void DeassignSingleton()
    {
        Instance = null;
    }

    private IEnumerator StartEnemySpawn()
    {
        WaveTimer = -2;
        yield return _map.SpawnEnemy(Wave, (e) => enemyList.Add(e));
        
        WaveTimer = _map.GetSpawnTiming(Wave).amount * _map.GetSpawnTiming(Wave).waitTimeMultiplier;
        _enemyCo = null;
    }

    public void NextWave()
    {
        if (Math.Abs(WaveTimer - (-2)) < 0.001f) return;
        if (_enemyCo != null) StopCoroutine(_enemyCo);
        Wave++;
        if (_map.ExpandThisWave(Wave))
            _map.ExpandMap(Wave);
        
        _enemyCo = StartCoroutine(StartEnemySpawn());
    }

    public bool Purchase(int amount)
    {
        if (Money < amount) return false;
        Money -= amount;
        return true;
    }
}
