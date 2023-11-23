using System;

public delegate void EnemyDeadEvent(Enemy e);

public delegate void EnemyReachedBaseEvent(Enemy e, int lifeCost);