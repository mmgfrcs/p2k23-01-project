using System;

public delegate void EnemyDeadEvent(EnemyType enemyType, float bounty);

public delegate void EnemyReachedBaseEvent(int lifeCost);