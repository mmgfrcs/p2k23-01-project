using AdInfinitum.Entities;

namespace AdInfinitum.Utilities
{
    public delegate void EnemyDeadEvent(Enemy e);

    public delegate void EnemyReachedBaseEvent(Enemy e, uint lifeCost);
}