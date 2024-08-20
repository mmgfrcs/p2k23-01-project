using System.Collections.Generic;
using System.Linq;
using AdInfinitum.Utilities;
using UnityEngine;

namespace AdInfinitum.Database
{
    [CreateAssetMenu(fileName = "ExpansionConfig", menuName = "Expansion Config", order = 1)]
    public class ExpansionConfig : ScriptableObject
    {
        public List<ExpansionTiming> expansionTimings;

        public IReadOnlyDictionary<uint, ExpansionTiming> expTimingDict;

        private void Awake()
        {
            expTimingDict = expansionTimings.ToDictionary(x => x.expandOnWave);
        }

        private void OnEnable()
        {
            expTimingDict = expansionTimings.ToDictionary(x => x.expandOnWave);
        }
    }
}
