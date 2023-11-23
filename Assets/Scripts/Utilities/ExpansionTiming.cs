
using System;
using UnityEngine;

[Serializable]
public struct ExpansionTiming
{
    public uint expandOnWave;
    public Vector2Int[] newCheckpoints;
    public Vector2Int[] newTowerSpots;
}
