using UnityEngine;

namespace Utilities
{
    [CreateAssetMenu(fileName = "SpawnConfig", menuName = "Spawn Configuration", order = 0)]
    public class SpawnConfig : ScriptableObject
    {
        public SpawnTiming[] spawnTimings;
    }
}