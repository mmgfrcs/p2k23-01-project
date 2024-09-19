using System;
using System.Linq;
using AdInfinitum.Entities;

namespace AdInfinitum.Utilities
{
    [Serializable]
    public struct SpawnTiming
    {
        public SpawnFormation[] formations;
        public float waitTimeMultiplier;

        public uint TotalAmount => formations.Sum(x => x.amount);

        public SpawnTimingYML ToYML()
        {
            return new SpawnTimingYML()
            {
                formations = formations.Select(x=>new SpawnFormationYML()
                {
                    enemyPrefab = x.enemyPrefab.Type,
                    amount = x.amount,
                    delay = x.delay,
                    health = x.health
                }).ToArray(),
                waitTimeMultiplier = waitTimeMultiplier
            };
        }
    }

    [Serializable]
    public struct SpawnTimingYML
    {
        public SpawnFormationYML[] formations;
        public float waitTimeMultiplier;
    }

    [Serializable]
    public struct SpawnFormation
    {
        public Enemy enemyPrefab;
        public float health;
        public float delay;
        public uint amount;
    }

    [Serializable]
    public struct SpawnFormationYML
    {
        public EnemyType enemyPrefab;
        public float health;
        public float delay;
        public uint amount;
    }
}