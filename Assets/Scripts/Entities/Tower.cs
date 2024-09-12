using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AdInfinitum.Managers;
using AdInfinitum.UI;
using AdInfinitum.Utilities;
using UnityEngine;
using UnityEngine.Pool;

namespace AdInfinitum.Entities
{
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

        [Serializable]
        public struct StatUpgrade
        {
            public string name;
            public StatType type;
            public float value;
        }

        public enum StatType
        {
            Generic, Slow, SplashRange
        }

        [Serializable]
        public enum Targeting
        {
            First, Last, Strongest, Weakest
        }

        public class TowerReport
        {
            public ulong Kills { get; private set; }
            public float DPS { get; set; }

            public void AddKill() => Kills++;
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

        [Header("Upgrades"), SerializeField] private float damageUpgradeRatio = 0.1f;
        [SerializeField] private float projectileSpeedUpgradeRatio;
        [SerializeField] private float rotationSpeedUpgradeRatio = 0.05f;
        [SerializeField] private float attackSpeedUpgradeRatio = 0.02f;
        [SerializeField] private float rangeUpgradeRatio = 0.05f;
        [SerializeField] private StatUpgrade[] otherStatsUpgradeRatio;

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

        public Targeting TowerTargeting { get; internal set; } = Targeting.First;

        /// <summary>
        /// Is the <see cref="Tower"/> locked onto a target?
        /// </summary>
        private bool _isLocked;
        private RangeCircle _rangeCircle;
        private readonly List<Collider2D> _scanResult = new();
        private ObjectPool<Bullet> _bulletPool;
        private float _cooldown;
        private bool _isGameOver;
        private int _tipIdx = 0;
        private float _baseSlow, _baseSplashRange;
        private bool _isCharging = true;

        private float _dmg;
        private int _hitAmt;

        private Dictionary<Targeting, Comparison<Enemy>> targetingSorter = new Dictionary<Targeting, Comparison<Enemy>>()
        {
            { Targeting.First , (first, second) => first.CompareTo(second)},
            { Targeting.Last , (first, second) => second.CompareTo(first)},
            { Targeting.Strongest , (first, second) =>
            {
                var dist = second.MaxHealth - first.MaxHealth;
                return dist switch
                {
                    > 0 => 1,
                    < 0 => -1,
                    _ => first.CompareTo(second)
                };
            }},
            { Targeting.Weakest , (first, second) =>
            {
                var dist = second.MaxHealth - first.MaxHealth;
                return dist switch
                {
                    > 0 => -1,
                    < 0 => 1,
                    _ => first.CompareTo(second)
                };
            }}
        };

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

            StartCoroutine(UpdateDPS());
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
                        .Where(x =>
                        {
                            var en = x.GetComponent<Enemy>();
                            return en != null && en.Health > 0;
                        })
                        .Select(x => x.GetComponent<Enemy>())
                        .ToList();

                    enemies.Sort(targetingSorter[TowerTargeting]);
                    var idx = 0;

                    if (enemies.Count == 0)
                    {
                        Target = null;
                        _isLocked = false;
                        return;
                    }
                    Target = enemies[idx];
                    while (GetEfficiency(Target.Type) <= 0)
                    {
                        idx++;
                        if(idx >= enemies.Count) break;
                        Target = enemies[idx];
                    }

                    if (idx >= enemies.Count)
                    {
                        Target = null;
                        _isLocked = false;
                    }
                    else _isLocked = true;
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
            if (chargeTime > 0 && _cooldown <= chargeTime && !_isCharging)
            {
                AudioManager.Instance.PlayTowerSFX(transform.position, type, EntitySFXType.Charge);
                _isCharging = true;
            }

            if (_cooldown > 0)
                return;

            Bullet b = _bulletPool.Get();
            b.transform.position = barrelTip[_tipIdx].transform.position;
            b.transform.rotation = barrelTip[_tipIdx].transform.rotation;
            b.Initialize(this, Target, Damage * GetEfficiency(Target.Type), ProjectileSpeed, otherStats.FirstOrDefault(x=>x.type == StatType.SplashRange).value);
            b.Hit += BulletOnHit;
            _cooldown = 1f / AttackSpeed;
            AudioManager.Instance.PlayTowerSFX(transform.position, type, EntitySFXType.Shoot);

            _tipIdx++;
            _isCharging = false;
            if (_tipIdx >= barrelTip.Length) _tipIdx = 0;
        }

        private void BulletOnHit(Bullet obj, int amt)
        {
            obj.Hit -= BulletOnHit;
            _hitAmt++;
            _dmg += obj.Damage * amt;
        }

        public void ReleaseBullet(Bullet b)
        {
            GameManager.Instance.SpawnExplosion(type, b.transform.position);
            _bulletPool.Release(b);
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
            SellPrice += Convert.ToUInt64(UpgradePrice / 2);
            Level++;
            Damage += DamageLevelUpEffect;
            RotationSpeed += RotationSpeedLevelUpEffect;
            AttackSpeed += AttackSpeedLevelUpEffect;
            Range += RangeLevelUpEffect;
            Slow += SlowLevelUpEffect;
            SplashRange += SplashRangeLevelUpEffect;
            ProjectileSpeed += ProjectileSpeedLevelUpEffect;
        }

        public void Sell()
        {
            GameManager.Instance.AddMoney(SellPrice);
            Destroy(gameObject);
        }

        public float DamageLevelUpEffect => damage * damageUpgradeRatio;
        public float RotationSpeedLevelUpEffect => rotationSpeed * rotationSpeedUpgradeRatio;
        public float AttackSpeedLevelUpEffect => attackSpeed * attackSpeedUpgradeRatio;
        public float RangeLevelUpEffect => range * rangeUpgradeRatio;

        private float _slowLvlUpEff = -1;
        public float SlowLevelUpEffect
        {
            get
            {
                if (_slowLvlUpEff < 0)
                    _slowLvlUpEff = _baseSlow * otherStatsUpgradeRatio.FirstOrDefault(x => x.type == StatType.Slow).value;
                return _slowLvlUpEff;
            }
        }
        private float _splLvlUpEff = -1;
        public float SplashRangeLevelUpEffect
        {
            get
            {
                if (_splLvlUpEff < 0)
                    _splLvlUpEff = _baseSplashRange * otherStatsUpgradeRatio.FirstOrDefault(x => x.type == StatType.SplashRange).value;
                return _splLvlUpEff;
            }
        }
        public float ProjectileSpeedLevelUpEffect => projectileSpeed * projectileSpeedUpgradeRatio;
        public float UpgradePrice => Mathf.Ceil(Price / 6f + Level * 9 + Level * Level);

        private IEnumerator UpdateDPS()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f);
                if (_towerReport.DPS == 0) _towerReport.DPS = _hitAmt == 0 ? 0 : _dmg / _hitAmt;
                else
                {
                    _towerReport.DPS -= _towerReport.DPS / 50;
                    _towerReport.DPS += (_hitAmt == 0 ? 0 : _dmg / _hitAmt) / 50;
                }
                _dmg = 0;
                _hitAmt = 0;
            }
        }
    }
}
