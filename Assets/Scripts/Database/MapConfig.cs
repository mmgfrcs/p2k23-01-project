using System.Collections.Generic;
using AdInfinitum.Utilities;
using UnityEngine;

namespace AdInfinitum.Database
{
    [CreateAssetMenu(fileName = "SpawnConfig", menuName = "Spawn Configuration", order = 0)]
    public class MapConfig : ScriptableObject
    {
        public List<SpawnTiming> spawnTimings;
    }
}
