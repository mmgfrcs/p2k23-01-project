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
        public StatType type;
        public Sprite icon;
        public float value;
        public bool isDecimal;
        public string unitString;
    }

    public enum StatType
    {
        Generic, Slow, SplashRange
    }

    public class TowerReport
    {
        private ulong kills;
        public ulong Kills => kills;
        public void AddKill() => kills++;
    }

    [Header("Statistics"), SerializeField] private TowerType type;
    [SerializeField] private uint price;
    [SerializeField] private float damage;
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float attackSpeed;
    [SerializeField] private float range;
    [SerializeField] private float chargeTime = 1;
    [SerializeField] private Stat[] otherStats;
    [SerializeField] private TowerEfficiency[] efficiencies;

    [Header("Prefabs"), SerializeField] private Transform barrelCenter;
    [SerializeField] private Transform[] barrelTip;
    [SerializeField] private Bullet bullet;
    
    [Header("Visual"), SerializeField] private Sprite icon;

    private TowerReport _towerReport = new TowerReport();
    private Dictionary<EnemyType, float> _efficiencyDict;

    public TowerType Type => type;
    public uint Price => price;
    public float BaseDamage => damage;
    public float BaseProjectileSpeed => projectileSpeed;
    public float BaseRotationSpeed => rotationSpeed;
    public float BaseAttackSpeed => attackSpeed;
    public float BaseRange => range;
    public float Damage { get; private set; }
    public float ProjectileSpeed { get; private set; }
    public float RotationSpeed { get; private set; }
    public float AttackSpeed { get; private set; }
    public float Range { get; private set; }
    public float Slow { get; private set; }
    public float SplashRange { get; private set; }
    public Sprite Icon => icon;
    public uint Level { get; private set; } = 1;
    public Stat[] OtherStatistics => otherStats;
    public TowerReport Reports => _towerReport;
    public ulong SellPrice { get; private set; }
    
    public Enemy Target { get; private set; }

    private bool _isLocked;
    private RangeCircle _rangeCircle;
    private List<Collider2D> _scanResult = new List<Collider2D>();
    private ObjectPool<Bullet> _bulletPool;
    private float _cooldown;
    private bool _isGameOver;
    private int _tipIdx = 0;
    private float _baseSlow, _baseSplashRange;

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

        Damage = damage;
        ProjectileSpeed = projectileSpeed;
        RotationSpeed = rotationSpeed;
        AttackSpeed = attackSpeed;
        Range = range;
        SellPrice = Price / 2;
        _baseSlow = otherStats.FirstOrDefault(x => x.type == StatType.Slow).value;
        _baseSplashRange = otherStats.FirstOrDefault(x => x.type == StatType.SplashRange).value;
        Slow = _baseSlow;
        SplashRange = _baseSplashRange;
        
        AudioManager.Instance.PlayTowerSFX(transform.position, type, EntitySFXType.Deploy);
    }

    private void OnGameOver(float delay)
    {
        _isGameOver = true;
        Target = null;
    }

    private void EnemyOnReachedBase(Enemy e, uint lifecost)
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

        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = true;
        filter.SetLayerMask(LayerMask.GetMask("Enemy"));
        
        if (Type == TowerType.Slow)
        {
            if (GameManager.Instance.EnemyAmount == 0) return;
            filter = new ContactFilter2D();
            filter.useTriggers = true;
            filter.SetLayerMask(LayerMask.GetMask("Enemy"));
            if (Physics2D.OverlapCircle(transform.position, Range, filter, _scanResult) > 0)
            {
                var enemies = _scanResult
                    .Where(x => x.GetComponent<Enemy>() != null)
                    .Select(x => x.GetComponent<Enemy>())
                    .ToList();

                for (int i = 0; i < enemies.Count; i++)
                    enemies[i].SetSpeedMultiplier(1 - otherStats.FirstOrDefault(x=>x.type == StatType.Slow).value/100);
            }
        }
        else
        {
            if (GameManager.Instance.EnemyAmount == 0) return;
    
            if (Physics2D.OverlapCircle(transform.position, Range, filter, _scanResult) > 0)
            {
                var enemies = _scanResult
                    .Where(x => x.GetComponent<Enemy>() != null)
                    .Select(x => x.GetComponent<Enemy>())
                    .ToList();
    
                enemies.Sort();
                Target = enemies[0];
                _isLocked = true;
            }
            else
            {
                Target = null;
                _isLocked = false;
                return;
            }
        }

        if (!_isLocked) return;
        
        Vector2 direction = Target.transform.position - transform.position;
        barrelCenter.rotation = Quaternion.RotateTowards(barrelCenter.rotation,
            Quaternion.FromToRotation(Vector3.up, direction), RotationSpeed * Time.deltaTime);

        if (Quaternion.Angle(barrelCenter.rotation, Quaternion.FromToRotation(Vector3.up, direction)) < 1f)
            Shoot();
    }

    private void Shoot()
    {
        if (chargeTime > 0 && _cooldown <= chargeTime) AudioManager.Instance.PlayTowerSFX(transform.position, type, EntitySFXType.Charge);
        
        if (_cooldown > 0)
            return;
        
        Bullet b = _bulletPool.Get();
        b.transform.position = barrelTip[_tipIdx].transform.position;
        b.transform.rotation = barrelTip[_tipIdx].transform.rotation;
        b.Initialize(Target, Damage * GetEfficiency(Target.Type), ProjectileSpeed, otherStats.FirstOrDefault(x=>x.type == StatType.SplashRange).value);
        b.Hit += BulletOnHit;
        _cooldown = 1f / AttackSpeed;
        AudioManager.Instance.PlayTowerSFX(transform.position, type, EntitySFXType.Shoot);

        _tipIdx++;
        if (_tipIdx >= barrelTip.Length) _tipIdx = 0;
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

    public void ShowRange()
    {
        _rangeCircle.SetRadius(Range);
        _rangeCircle.ShowLine();
    }

    public void HideRange()
    {
        _rangeCircle.HideLine();
    }

    public void LevelUp()
    {
        SellPrice += Convert.ToUInt64(GetUpgradePrice() / 2);
        Level++;
        Damage += GetDamageLevelUpEffect();
        RotationSpeed += GetRotationSpeedLevelUpEffect();
        AttackSpeed += GetAttackSpeedLevelUpEffect();
        Range += GetRangeLevelUpEffect();
        Slow += GetSlowLevelUpEffect();
        SplashRange += GetSplashRangeLevelUpEffect();
        ProjectileSpeed += GetProjectileSpeedLevelUpEffect();
    }

    public void Sell()
    {
        GameManager.Instance.AddMoney(SellPrice);
        Destroy(gameObject);
    }

    public float GetDamageLevelUpEffect() => damage * 0.1f;
    public float GetRotationSpeedLevelUpEffect() => rotationSpeed * 0.05f;
    public float GetAttackSpeedLevelUpEffect() => attackSpeed * 0.02f;
    public float GetRangeLevelUpEffect() => range * 0.02f;
    public float GetSlowLevelUpEffect() => _baseSlow * 0.075f;
    public float GetSplashRangeLevelUpEffect() => 0;
    public float GetProjectileSpeedLevelUpEffect() => 0;
    public float GetUpgradePrice() => Mathf.Ceil(Price / 4f + Level * 10 + Level * Level);
}
