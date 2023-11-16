using System;
using UnityEngine;

namespace Entities
{
    public class Enemy : MonoBehaviour
    {
        [SerializeField] private string enemyName;
        [SerializeField] private float speed;

        public string Name => enemyName;
        public float Health { get; private set; } = 0;
        public float Bounty { get; private set; } = 0;
        
        public static event Action<string, float> Death;
        
        private Grid _parentGrid;
        private bool _isInitialized;
        
        public void Initialize(Grid grid, float health, float bounty)
        {
            _parentGrid = grid;
            Health = health;
            Bounty = bounty;
            _isInitialized = true;
        }
        
        private void Update()
        {
            if (!_isInitialized) return;
            if (Health <= 0)
            {
                Death?.Invoke(Name, Bounty);
                _isInitialized = false;
            }
        }
    }
}