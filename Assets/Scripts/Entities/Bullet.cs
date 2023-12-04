using System;
using System.Collections;
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

    public void Initialize(Enemy target, float damage, float speed)
    {
        Damage = damage;
        Speed = speed;
        _target = target;
        _isInitialized = true;
    }

    private void Update()
    {
        if (!_isInitialized) return;
        var dir = _target.transform.position - transform.position;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.FromToRotation(Vector3.up, dir),
            90 * Time.deltaTime);
        transform.Translate(Vector3.up * (Speed * Time.deltaTime));
        
        if (!_target.isActiveAndEnabled)
            lifetime = 1;
        
        if (lifetime > 0) lifetime -= Time.deltaTime;
        else Kill();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!_isInitialized) return;
        Enemy e = col.GetComponent<Enemy>();
        if (e != null && e == Target)
        {
            e.Damage(Damage);
            Kill();
        }
    }

    private void Kill()
    {
        StopAllCoroutines();
        Hit?.Invoke(this);
        _isInitialized = false;
    }
}
