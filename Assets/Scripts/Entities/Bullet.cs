using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float lifetime = 5f;
    public float Damage { get; private set; }
    public float Speed { get; private set; }
    public Enemy Target => _target;
    public Tower Owner => _parent;

    public event Action<Bullet, int> Hit;
    private Enemy _target;
    private Tower _parent;
    private bool _isInitialized;
    private float _lifetimeRemaining;
    private float _splashRange;
    private List<Collider2D> _scanResult = new List<Collider2D>();

    private Vector3 _lastPos;

    private TrailRenderer _trail;

    private void Awake()
    {
        _trail = GetComponentInChildren<TrailRenderer>();
    }

    public void Initialize(Tower parent, Enemy target, float damage, float speed, float splashRange)
    {
        Damage = damage;
        Speed = speed;
        _target = target;
        _parent = parent;
        _lifetimeRemaining = lifetime;
        _isInitialized = true;
        _splashRange = splashRange;
        _trail.transform.parent = transform;
        _trail.transform.localPosition = Vector3.zero;
        _trail.Clear();
    }

    private void Update()
    {
        if (!_isInitialized) return;
        if (_target != null)
        {
            var dir = _target.transform.position - transform.position;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.FromToRotation(Vector3.up, dir),
                90 * Time.deltaTime);
            
            if (!_target.isActiveAndEnabled)
            {
                if (_lifetimeRemaining > 1)
                {
                    if (_splashRange > 0) _lastPos = _target.transform.position;
                    else _lifetimeRemaining = 1;
                }
                _target = null;
            }
        } 
        else if (_lastPos != Vector3.zero)
        {
            var dir = _lastPos - transform.position;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.FromToRotation(Vector3.up, dir),
                90 * Time.deltaTime);
        }

        transform.Translate(Vector3.up * (Speed * Time.deltaTime));

        if(_lastPos != Vector3.zero && Vector3.Distance(_lastPos, transform.position) <= 0.01f) Kill(true);
        
        if (_lifetimeRemaining > 0) _lifetimeRemaining -= Time.deltaTime;
        else if (_lastPos == Vector3.zero) Kill();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!_isInitialized) return;
        Enemy e = col.GetComponent<Enemy>();
        
        if (e == null || e != Target) return;
        
        e.Damage(Owner, Damage);

        var amt = 1;
        
        if (_splashRange > 0) amt += SplashDamage(e);
        
        Kill(true, amt);
    }

    private int SplashDamage(Enemy exception = null)
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = true;
        filter.SetLayerMask(LayerMask.GetMask("Enemy"));
                
        if (Physics2D.OverlapCircle(transform.position, _splashRange, filter, _scanResult) == 0) return 0;
                
        var enemies = _scanResult
            .Where(x => x.GetComponent<Enemy>() != null)
            .Select(x => x.GetComponent<Enemy>())
            .Where(x=> x != exception);

        var n = 0;
        foreach (var enemy in enemies)
        {
            enemy.Damage(Owner, Damage);
            n++;
        }

        return n;
    }

    private void Kill(bool hit = false, int amt = 1)
    {
        StopAllCoroutines();
        if (_trail != null) _trail.transform.parent = null;
        var splash = false;
        if (_lastPos != Vector3.zero && _splashRange > 0)
        {
            splash = true;
            amt = SplashDamage();
        }
        
        _lastPos = Vector3.zero;
        
        if(hit || splash) Hit?.Invoke(this, amt);
        _parent.ReleaseBullet(this);
        _parent = null;
        _target = null;
        _isInitialized = false;
    }
}
