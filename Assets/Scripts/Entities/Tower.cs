using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(typeof(RangeCircle))]
public class Tower : MonoBehaviour
{
    [Serializable]
    public struct Stat
    {
        public string name;
        public Sprite icon;
        public float value;
        public bool isDecimal;
        public string unitString;
    }

    public class TowerReport
    {
        private ulong kills;
        public ulong Kills => kills;
        public void AddKill() => kills++;
    }

    [Header("Statistics"), SerializeField] private TowerType type;
    [SerializeField] private int price;
    [SerializeField] private float damage;
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float attackSpeed;
    [SerializeField] private float range;
    [SerializeField] private Stat[] otherStats;
    [SerializeField] private TowerEfficiency[] efficiencies;

    [Header("Prefabs"), SerializeField] private Transform barrelCenter;
    [SerializeField] private Transform barrelTip;
    [SerializeField] private Bullet bullet;
    
    [Header("Visual"), SerializeField] private Sprite icon;

    private TowerReport _towerReport = new TowerReport();
    private Dictionary<EnemyType, float> _efficiencyDict;

    public TowerType Type => type;
    public int Price => price;
    public float Damage => damage;
    public float ProjectileSpeed => projectileSpeed;
    public float RotationSpeed => rotationSpeed;
    public float AttackSpeed => attackSpeed;
    public float Range => range;
    public Sprite Icon => icon;
    public Stat[] OtherStatistics => otherStats;
    public TowerReport Reports => _towerReport;

    public Enemy Target { get; private set; }

    private RangeCircle _rangeCircle;
    private List<Collider2D> _scanResult = new List<Collider2D>();
    private ObjectPool<Bullet> _bulletPool;
    private float _cooldown;
    private bool _isGameOver;

    // Start is called before the first frame update
    private void Start()
    {
        Enemy.Death += EnemyOnDeath;
        Enemy.ReachedBase += EnemyOnReachedBase;
        _efficiencyDict ??= efficiencies.ToDictionary(x => x.enemy, x => x.efficiency);
        
        _bulletPool = new ObjectPool<Bullet>(() =>
            Instantiate(bullet.gameObject, Vector3.zero, Quaternion.identity).GetComponent<Bullet>(),
            b => b.gameObject.SetActive(true),
            b => b.gameObject.SetActive(false),
            b => Destroy(b.gameObject));

        _rangeCircle = GetComponent<RangeCircle>();
        _rangeCircle.SetRadius(range);
        _rangeCircle.HideLine();
        GameManager.Instance.GameOver += OnGameOver;
    }

    private void OnGameOver()
    {
        _isGameOver = true;
        Target = null;
    }

    private void EnemyOnReachedBase(Enemy e, int lifecost)
    {
        if (Target == e) Target = null;
    }

    private void EnemyOnDeath(Enemy e)
    {
        if (Target == e) Target = null;
    }

    // Update is called once per frame
    private void Update()
    {
        if (_isGameOver) return;
        
        if (_cooldown > 0) _cooldown -= Time.deltaTime;
        
        if (Type == TowerType.Slow)
        {
            if (GameManager.Instance.EnemyAmount == 0) return;
            var filter = new ContactFilter2D();
            filter.useTriggers = true;
            filter.SetLayerMask(LayerMask.GetMask("Enemy"));
            if (Physics2D.OverlapCircle(transform.position, range, filter, _scanResult) > 0)
            {
                var enemies = _scanResult
                    .Where(x => x.GetComponent<Enemy>() != null)
                    .Select(x => x.GetComponent<Enemy>())
                    .ToList();

                for (int i = 0; i < enemies.Count; i++)
                    enemies[i].SetSpeedMultiplier(1 - (otherStats[0].value/100));
            }
        }
        
        if (Target == null)
        {
            if (GameManager.Instance.EnemyAmount == 0) return;
            var filter = new ContactFilter2D();
            filter.useTriggers = true;
            filter.SetLayerMask(LayerMask.GetMask("Enemy"));
            if (Physics2D.OverlapCircle(transform.position, range, filter, _scanResult) > 0)
            {
                var enemies = _scanResult
                    .Where(x => x.GetComponent<Enemy>() != null)
                    .Select(x => x.GetComponent<Enemy>())
                    .ToList();

                enemies.Sort();
                Target = enemies[0];
            }
            else return;
        }

        Vector2 direction = Target.transform.position - transform.position;
        barrelCenter.rotation = Quaternion.RotateTowards(barrelCenter.rotation,
            Quaternion.FromToRotation(Vector3.up, direction), rotationSpeed * Time.deltaTime);

        if (Quaternion.Angle(barrelCenter.rotation, Quaternion.FromToRotation(Vector3.up, direction)) < 1f)
            Shoot();
    }

    private void Shoot()
    {
        if (_cooldown > 0)
            return;
        
        Bullet b = _bulletPool.Get();
        b.transform.position = barrelTip.transform.position;
        b.transform.rotation = barrelTip.transform.rotation;
        b.Initialize(Target, damage * GetEfficiency(Target.Type), projectileSpeed);
        b.Hit += BulletOnHit; 
        _cooldown = 1f / attackSpeed;
    }

    private void BulletOnHit(Bullet obj)
    {
        obj.Hit -= BulletOnHit;
        _bulletPool.Release(obj);
        if (obj.Target.Health <= 0) _towerReport.AddKill();
    }

    public float GetEfficiency(EnemyType enemy)
    {
        _efficiencyDict ??= efficiencies.ToDictionary(x => x.enemy, x => x.efficiency);
        if (_efficiencyDict.TryGetValue(enemy, out var eff)) return eff;
        return 1;
    }
}
