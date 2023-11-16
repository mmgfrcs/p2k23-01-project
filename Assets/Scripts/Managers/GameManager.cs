using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    [Header("Configuration"), SerializeField] private int startingLife = 10;
    [SerializeField] private int startingMoney = 150;
    
    public int Score { get; private set; }
    public int Money { get; private set; }
    public int Life { get; private set; }
    public int Wave { get; private set; }
    public int EnemyAmount { get; private set; }
    public float WaveTimer { get; private set; }
    
    public static GameManager Instance { get; private set; }

    private Map _map;
    
    // Start is called before the first frame update
    private void Start()
    {
        AssignSingleton();
        Life = startingLife;
        Money = startingMoney;
        Score = 0;
        Wave = 1;
        EnemyAmount = 0;
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
}
