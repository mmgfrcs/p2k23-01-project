using AdInfinitum.Managers;
using UnityEngine;

namespace AdInfinitum.UI
{
    public class WaveIndicatorGroup : MonoBehaviour
    {
        [SerializeField] private WaveIndicator[] indicators;
        private MapManager _map;

        private void Start()
        {
            _map = FindObjectOfType<MapManager>();
        }

        private void Update()
        {
            for (uint i = 0; i < indicators.Length; i++)
            {
                var wave = GameManager.Instance.Wave + i + 1;
                var timing = _map.GetSpawnTiming(wave);
            
                indicators[i].Set(wave, timing.formations, GameManager.Instance.CurrentMap.ExpandThisWave(wave));
            }
        }
    }
}
