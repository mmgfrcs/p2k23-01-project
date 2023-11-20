using System;
using UnityEngine;

namespace Entities
{
    public class Enemy : MonoBehaviour
    {
        [SerializeField] private EnemyType type;
        [SerializeField] private float speed = 1;

        public EnemyType Type => type;
        public float Health { get; private set; } = 0;
        public float Bounty { get; private set; } = 0;
        
        public static event EnemyDeadEvent Death;
        public static event EnemyReachedBaseEvent ReachedBase;

        public event Action<Enemy> JourneyComplete;
        
        private Grid _parentGrid;
        private bool _isInitialized;
        private Vector2Int[] _checkpoints;
        private int _checkpointIdx;
        
        public void Initialize(Grid grid, Vector2Int[] checkpoints, float health, float bounty)
        {
            _parentGrid = grid;
            Health = health;
            Bounty = bounty;
            _isInitialized = true;
            _checkpoints = checkpoints;
            _checkpointIdx = 0;
        }
        
        private void Update()
        {
            if (!_isInitialized) return;
            if (Health <= 0)
            {
                Kill();
                return;
            }

            var dest = _parentGrid.GetCellCenterWorld(_checkpoints[_checkpointIdx].ToVector3Int());
            var dir = (dest - transform.position).normalized;
            transform.Translate(dir * (speed * Time.deltaTime));

            if (Vector3.Distance(dest, transform.position) < 0.01f)
            {
                _checkpointIdx++;
                if (_checkpointIdx >= _checkpoints.Length)
                {
                    //Reached base... should be
                    ReachedBase?.Invoke(1);
                    _isInitialized = false;
                    JourneyComplete?.Invoke(this);
                }
            }
        }

        public void Kill()
        {
            Health = 0;
            Death?.Invoke(type, Bounty);
            _isInitialized = false;
            JourneyComplete?.Invoke(this);
        }
    }
}