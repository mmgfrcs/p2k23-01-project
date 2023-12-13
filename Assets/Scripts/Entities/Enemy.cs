using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour, IComparable<Enemy>, IEquatable<Enemy>
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private bool debugMode;
    [SerializeField] private EnemyType type;
    [SerializeField] private float speed = 1;
    [SerializeField] private uint lifeCost = 1;
    [SerializeField] private Slider hpBar;
    [SerializeField] private TextMeshProUGUI debugText;

    public EnemyType Type => type;
    public string EnemyID { get; private set; }
    public float Health { get; private set; } = 0;
    public float MaxHealth { get; private set; } = 0;
    public float Bounty { get; private set; } = 0;
    public float Distance { get; private set; } = 0;
    public float SpeedMultiplier { get; private set; } = 0;
    
    public static event EnemyDeadEvent Death;
    public static event EnemyReachedBaseEvent ReachedBase;
    public event Action<Enemy> JourneyComplete;
    
    private Grid _parentGrid;
    private bool _isInitialized;
    private Vector2Int[] _checkpoints;
    private int _checkpointIdx;
    private float _speedMultCooldown;
    private float _distanceTraveled;
    private Queue<float> _distances = new();
    private Vector3 _offset;
    private CanvasGroup _hpBarGroup;

    private void Awake()
    {
        if (!debugMode) Destroy(debugText.gameObject);
    }

    public void Initialize(Grid grid, Vector2Int[] checkpoints, Vector3 offset, float health, float bounty)
    {
        _parentGrid = grid;
        MaxHealth = health;
        Health = health;
        Bounty = bounty;
        _isInitialized = true;
        _checkpoints = checkpoints;
        _checkpointIdx = 0;
        EnemyID = Guid.NewGuid().ToString();
        _distanceTraveled = 0;
        Distance = 0;
        _offset = offset;
        _distances.Clear();
        _distanceTraveled = 0;
        hpBar.maxValue = health;
        hpBar.value = health;
        _hpBarGroup = hpBar.GetComponent<CanvasGroup>();
        _hpBarGroup.alpha = 0;
        var lastPos = transform.position;
        for (int i = 0; i < _checkpoints.Length; i++)
        {
            Vector3 chkpt = _parentGrid.GetCellCenterWorld(_checkpoints[i].ToVector3Int());
            
            Distance += Vector3.Distance(lastPos, chkpt);
            _distances.Enqueue(Distance);
            lastPos = chkpt;
        }
        
        GameManager.Instance.GameOver += OnGameOver;
        AudioManager.Instance.PlayEnemySFX(transform.position, type, EntitySFXType.Deploy);
    }

    private void OnGameOver(float delay)
    {
        DOTween.To(() => speed, value => speed = value, 0f, delay).SetUpdate(true);
    }

    private void Update()
    {
        if (!_isInitialized) return;

        if (Health <= 0)
        {
            AudioManager.Instance.PlayEnemySFX(transform.position, type, EntitySFXType.Destroy);
            Kill();
            return;
        }

        if (debugMode)
            debugText.text = $"C{_checkpointIdx+1} {_distanceTraveled:N2}/{_distances.Peek():N2} {Distance:N2}";

        var dest = _parentGrid.GetCellCenterWorld(_checkpoints[_checkpointIdx].ToVector3Int()) + _offset;
        var dir = (dest - transform.position).normalized;
        spriteRenderer.transform.rotation = Quaternion.LookRotation(Vector3.forward, dir) * Quaternion.Euler(0f, 0f, 90f);
        transform.Translate(dir * (speed * SpeedMultiplier * Time.deltaTime));

        Distance -= speed * SpeedMultiplier * Time.deltaTime;
        _distanceTraveled += speed * SpeedMultiplier * Time.deltaTime;

        if (Vector3.Distance(transform.position, dest) <= 0.01f)
        {
            _checkpointIdx++;
            if (_checkpointIdx >= _checkpoints.Length)
            {
                //Reached base... should be
                ReachedBase?.Invoke(this, lifeCost);
                _isInitialized = false;
                JourneyComplete?.Invoke(this);
            }
            else _distances.Dequeue();
        }

        if (_speedMultCooldown > 0) _speedMultCooldown -= Time.deltaTime;
        else SpeedMultiplier = 1;
    }

    public void Damage(float amount)
    {
        spriteRenderer.color = new Color(0.5f,0.5f,0.5f,0.75f);
        spriteRenderer.DOColor(Color.white, 0.6f);
        Health -= amount;
        _hpBarGroup.alpha = 1;
        if(Health > 0) hpBar.DOValue(Health, 0.4f);
    }

    public void Kill()
    {
        Health = 0;
        Death?.Invoke(this);
        _isInitialized = false;
        JourneyComplete?.Invoke(this);
        GameManager.Instance.GameOver -= OnGameOver;
    }

    public int CompareTo(Enemy other)
    {
        var dist = Distance - other.Distance;
        if (dist > 0) return 1;
        if (dist < 0) return -1;
        return 0;
    }

    public bool Equals(Enemy other)
    {
        return other != null && EnemyID == other.EnemyID;
    }

    public void SetSpeedMultiplier(float mult)
    {
        SpeedMultiplier = mult;
        _speedMultCooldown = 0.5f;
    }
}
