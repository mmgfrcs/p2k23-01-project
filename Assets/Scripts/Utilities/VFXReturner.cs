using System;
using AdInfinitum.Entities;
using AdInfinitum.Managers;
using UnityEngine;

namespace AdInfinitum.Utilities
{
    public class VFXReturner : MonoBehaviour
    {
        [SerializeField] private float delay = 1f;
        private float _currDelay;

        private TowerType _forTowerType;
        private EnemyType _forEnemyType; //Remains unused for now
        private bool? isTowerType;

        private void OnEnable()
        {
            _currDelay = delay;
        }

        private void Update()
        {
            if (_currDelay <= 0)
            {
                //For next expansion
                if(isTowerType.HasValue && isTowerType.Value) 
                    GameManager.Instance.ReleaseExplosion(_forTowerType, gameObject);
            }
            else _currDelay -= Time.deltaTime;
        }

        public void SetTargetType(TowerType type)
        {
            _forTowerType = type;
            isTowerType = true;
        }

        public void SetTargetType(EnemyType type)
        {
            _forEnemyType = type;
            isTowerType = false;
        }
    }
}