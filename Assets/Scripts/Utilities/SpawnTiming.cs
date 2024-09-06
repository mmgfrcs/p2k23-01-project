using System;
using AdInfinitum.Entities;

namespace AdInfinitum.Utilities
{
    [Serializable]
    public struct SpawnTiming
    {
        public Enemy enemyPrefab;
        public float waitTimeMultiplier;
        public float health;
        public float delay;
        public uint amount;

        public SpawnTimingYML ToYML()
        {
            return new SpawnTimingYML()
            {
                enemyPrefab = enemyPrefab.Type,
                amount = amount,
                delay = delay,
                health = health,
                waitTimeMultiplier = waitTimeMultiplier
            };
        }
    }

    [Serializable]
    public struct SpawnTimingYML
    {
        public EnemyType enemyPrefab;
        public float waitTimeMultiplier;
        public float health;
        public float delay;
        public uint amount;
    }
}