﻿using AdInfinitum.Managers;
using UnityEngine;

namespace AdInfinitum.UI
{
    public class WaveIndicatorGroup : MonoBehaviour
    {
        [SerializeField] private WaveIndicator[] indicators;
        private Map.Map _map;

        private void Start()
        {
            _map = FindObjectOfType<Map.Map>();
        }

        private void Update()
        {
            for (uint i = 0; i < indicators.Length; i++)
            {
                var wave = GameManager.Instance.Wave + i + 1;
                var timing = _map.GetSpawnTiming(wave);
            
                indicators[i].Set(timing.enemyPrefab.Type, wave, timing.amount, GameManager.Instance.CurrentMap.ExpandThisWave(wave));
            }
        }
    }
}
