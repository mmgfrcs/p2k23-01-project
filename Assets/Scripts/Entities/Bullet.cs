using System;
using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float lifetime = 5f;
    public float Damage { get; private set; }
    public float Speed { get; private set; }

    public event Action<Bullet> Hit;

    private Enemy _target;

    private bool _isInitialized;

    public void Initialize(Enemy target, float damage, float speed)
    {
        Damage = damage;
        Speed = speed;
        _target = target;
        _isInitialized = true;
        StartCoroutine(Lifetime());
    }

    private void Update()
    {
        if (!_isInitialized) return;
        var dir = _target.transform.position - transform.position;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.FromToRotation(Vector3.up, dir),
            90 * Time.deltaTime);
        transform.Translate(Vector3.up * (Speed * Time.deltaTime));
        if (!_target.isActiveAndEnabled)
            Kill();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!_isInitialized) return;
        Enemy e = col.GetComponent<Enemy>();
        if (e != null)
        {
            e.Damage(Damage);
            Kill();
        }
    }

    private IEnumerator Lifetime()
    {
        yield return new WaitForSeconds(lifetime);
        Kill();
    }

    private void Kill()
    {
        StopAllCoroutines();
        Hit?.Invoke(this);
        _isInitialized = false;
    }
}
