using System;
using System.Collections;
using System.Collections.Generic;
using Entities;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [Header("Configuration"), SerializeField] private int startingLife = 10;
    [SerializeField] private int startingMoney = 150;
    [SerializeField] private Map[] maps;
    
    public int Score { get; private set; }
    public int Money { get; private set; }
    public int Life { get; private set; }
    public int Wave { get; private set; }
    public int EnemyAmount { get; private set; }
    public float WaveTimer { get; private set; } = -1;
    
    public static GameManager Instance { get; private set; }

    private Map _map;
    private Coroutine _enemyCo;
    
    // Start is called before the first frame update
    private void Start()
    {
        AssignSingleton();
        Life = startingLife;
        Money = startingMoney;
        Score = 0;
        Wave = 1;
        EnemyAmount = 0;
        
        Enemy.ReachedBase += EnemyOnReachedBase;

        _map = Instantiate(maps[Random.Range(0, maps.Length)].gameObject, Vector3.zero, Quaternion.identity).GetComponent<Map>();
    }

    private void EnemyOnReachedBase(int lifecost)
    {
        Life -= lifecost;
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
        yield return _map.SpawnEnemy(1);
        WaveTimer = 8f;
        _enemyCo = null;
    }

    public void NextWave()
    {
        if (_enemyCo != null) StopCoroutine(_enemyCo);
        _enemyCo = StartCoroutine(StartEnemySpawn());
    }
}
