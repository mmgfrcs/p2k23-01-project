using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float lifetime = 5f;
    public float Damage { get; private set; }
    public float Speed { get; private set; }
    public Enemy Target => _target;

    public event Action<Bullet> Hit;
    private Enemy _target;
    private bool _isInitialized;
    private float _lifetimeRemaining;
    private float _splashRange;
    private List<Collider2D> _scanResult = new List<Collider2D>();

    public void Initialize(Enemy target, float damage, float speed, float splashRange)
    {
        Damage = damage;
        Speed = speed;
        _target = target;
        _lifetimeRemaining = lifetime;
        _isInitialized = true;
        _splashRange = splashRange;
    }

    private void Update()
    {
        if (!_isInitialized) return;
        var dir = _target.transform.position - transform.position;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.FromToRotation(Vector3.up, dir),
            90 * Time.deltaTime);
        transform.Translate(Vector3.up * (Speed * Time.deltaTime));
        
        if (!_target.isActiveAndEnabled && _lifetimeRemaining > 1)
            _lifetimeRemaining = 1;
        
        if (_lifetimeRemaining > 0) _lifetimeRemaining -= Time.deltaTime;
        else Kill();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!_isInitialized) return;
        Enemy e = col.GetComponent<Enemy>();
        
        if (e == null || e != Target) return;
        
        e.Damage(Damage);
        
        if (_splashRange > 0)
        {
            ContactFilter2D filter = new ContactFilter2D();
            filter.useTriggers = true;
            filter.SetLayerMask(LayerMask.GetMask("Enemy"));
                
            if (Physics2D.OverlapCircle(transform.position, _splashRange, filter, _scanResult) == 0) return;
                
            var enemies = _scanResult
                .Where(x => x.GetComponent<Enemy>() != null)
                .Select(x => x.GetComponent<Enemy>());

            foreach (var enemy in enemies)
            {
                if (enemy == e) continue;
                enemy.Damage(Damage);
            }
                
        }
        
        Kill();
    }

    private void Kill()
    {
        StopAllCoroutines();
        Hit?.Invoke(this);
        _isInitialized = false;
    }
}
