using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(typeof(RangeCircle))]
public class Tower : MonoBehaviour
{
    [Serializable]
    public struct Stat
    {
        public string name;
        public float value;
        public string unitString;
    }

    [Header("Statistics"), SerializeField] private float damage;
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float attackSpeed;
    [SerializeField] private float range;
    [SerializeField] private Stat[] otherStats;

    [Header("Prefabs"), SerializeField] private Transform barrelCenter;
    [SerializeField] private Transform barrelTip;
    [SerializeField] private Bullet bullet;
    
    [Header("Visual"), SerializeField] private Sprite icon;

    public float Damage => damage;
    public float ProjectileSpeed => projectileSpeed;
    public float RotationSpeed => rotationSpeed;
    public float AttackSpeed => attackSpeed;
    public float Range => range;
    public Sprite Icon => icon;
    public Stat[] OtherStatistics => otherStats;

    public Enemy Target { get; private set; }

    private RangeCircle rangeCircle;
    private List<Collider2D> scanResult = new List<Collider2D>();
    private ObjectPool<Bullet> bulletPool;
    private float cooldown;

    // Start is called before the first frame update
    private void Start()
    {
        Enemy.Death += EnemyOnDeath;
        Enemy.ReachedBase += EnemyOnReachedBase;
        bulletPool = new ObjectPool<Bullet>(() =>
            Instantiate(bullet.gameObject, Vector3.zero, Quaternion.identity).GetComponent<Bullet>(),
            b => b.gameObject.SetActive(true),
            b => b.gameObject.SetActive(false),
            b => Destroy(b.gameObject));

        rangeCircle = GetComponent<RangeCircle>();
        rangeCircle.SetRadius(range);
        rangeCircle.HideLine();
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
        if (cooldown > 0) cooldown -= Time.deltaTime;
        
        if (Target == null)
        {
            if (GameManager.Instance.EnemyAmount == 0) return;
            var filter = new ContactFilter2D();
            filter.useTriggers = true;
            filter.SetLayerMask(LayerMask.GetMask("Enemy"));
            if (Physics2D.OverlapCircle(transform.position, range, filter, scanResult) > 0)
            {
                var enemies = scanResult
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
        if (cooldown > 0)
            return;
        
        Bullet b = bulletPool.Get();
        b.transform.position = barrelTip.transform.position;
        b.transform.rotation = barrelTip.transform.rotation;
        b.Initialize(Target, damage, projectileSpeed);
        b.Hit += BulletOnHit;
        cooldown = 1f / attackSpeed;
    }

    private void BulletOnHit(Bullet obj)
    {
        obj.Hit -= BulletOnHit;
        bulletPool.Release(obj);
    }
}
