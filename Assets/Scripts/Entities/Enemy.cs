using System;
using System.Collections;
using System.Collections.Generic;
using AdInfinitum.Managers;
using AdInfinitum.Utilities;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AdInfinitum.Entities
{
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

        private Collider2D _collider;
        private Grid _parentGrid;
        private bool _isInitialized;
        private Queue<Vector2Int> _checkpoints;
        private int _checkpointIdx;
        private float _speedMultCooldown;
        private float _distanceTraveled;
        private Vector3 _offset;
        private CanvasGroup _hpBarGroup;
        private Animator _anim;

        private void Awake()
        {
            if (!debugMode) Destroy(debugText.gameObject);
            _anim = spriteRenderer.GetComponent<Animator>();
            _collider = GetComponent<Collider2D>();
        }

        public void Initialize(Grid grid, Vector2Int[] checkpoints, Vector3 offset, float health, float bounty)
        {
            _parentGrid = grid;
            MaxHealth = health;
            Health = health;
            Bounty = bounty;
            _isInitialized = true;
            _collider.enabled = true;
            _checkpoints = new Queue<Vector2Int>(checkpoints);
            _checkpointIdx = 0;
            EnemyID = Guid.NewGuid().ToString();
            _distanceTraveled = 0;
            Distance = 0;
            _offset = offset;
            _distanceTraveled = 0;
            hpBar.maxValue = health;
            hpBar.value = health;
            _hpBarGroup = hpBar.GetComponent<CanvasGroup>();
            _hpBarGroup.alpha = 0;
            var lastPos = transform.position;
            for (int i = 0; i < checkpoints.Length; i++)
            {
                Vector3 chkpt = _parentGrid.GetCellCenterWorld(checkpoints[i].ToVector3Int());

                Distance += Vector3.Distance(lastPos, chkpt);
                lastPos = chkpt;
            }
            _anim.Play("Idle");

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

            if (debugMode)
                debugText.text = $"C{_checkpointIdx+1} T{_distanceTraveled:N2}m R{Distance:N2}m";

            var dest = _parentGrid.GetCellCenterWorld(_checkpoints.Peek().ToVector3Int()) + _offset;
            var dir = (dest - transform.position).normalized;
            if(type != EnemyType.Boss)
                spriteRenderer.transform.rotation = Quaternion.LookRotation(Vector3.forward, dir) * Quaternion.Euler(0f, 0f, 90f);
            transform.Translate(dir * (speed * SpeedMultiplier * Time.deltaTime));

            Distance -= speed * SpeedMultiplier * Time.deltaTime;
            _distanceTraveled += speed * SpeedMultiplier * Time.deltaTime;

            if (Vector3.Distance(transform.position, dest) <= 0.01f)
            {
                _checkpointIdx++;
                _checkpoints.Dequeue();
                if (_checkpoints.Count == 0)
                {
                    //Reached base... should be
                    ReachedBase?.Invoke(this, lifeCost);
                    _isInitialized = false;
                    JourneyComplete?.Invoke(this);
                }
            }

            if (_speedMultCooldown > 0) _speedMultCooldown -= Time.deltaTime;
            else SpeedMultiplier = 1;
        }

        public void Damage(Tower source, float amount)
        {
            spriteRenderer.DOKill();
            spriteRenderer.color = new Color(0.5f,0.5f,0.5f,0.75f);
            spriteRenderer.DOColor(Color.white, 0.6f);
            Health -= amount;

            if (Health > 0)
            {
                hpBar.DOValue(Health, 0.4f);
                _hpBarGroup.alpha = 1;
            }
            else
            {
                source.Reports.AddKill();
                Kill();
            }
        }

        public void Kill()
        {
            Health = 0;
            _hpBarGroup.alpha = 0;
            Death?.Invoke(this);
            _isInitialized = false;
            GameManager.Instance.GameOver -= OnGameOver;
            _anim.Play("Death");
            AudioManager.Instance.PlayEnemySFX(transform.position, type, EntitySFXType.Destroy);
            _collider.enabled = false;
            
            StartCoroutine(WaitForAnimation((() =>
            {
                JourneyComplete?.Invoke(this);

            })));

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
            if(!Mathf.Approximately(mult, SpeedMultiplier))
                SpeedMultiplier = mult;
            
            _speedMultCooldown = 0.5f;
        }

        private IEnumerator WaitForAnimation(Action wtd)
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitUntil(() => _anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f );
            wtd.Invoke();
        }
    }
}
