
using System;
using UnityEngine;

[Serializable]
public struct ExpansionTiming: IEquatable<ExpansionTiming>
{
    public uint expandOnWave;
    public Vector2Int[] newCheckpoints;
    public Vector2Int[] newTowerSpots;

    public bool Equals(ExpansionTiming other)
    {
        return expandOnWave == other.expandOnWave  && Equals(newCheckpoints, other.newCheckpoints) && Equals(newTowerSpots, other.newTowerSpots);
    }

    public override bool Equals(object obj)
    {
        return obj is ExpansionTiming other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(expandOnWave, newCheckpoints, newTowerSpots);
    }

    public static bool operator ==(ExpansionTiming left, ExpansionTiming right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ExpansionTiming left, ExpansionTiming right)
    {
        return !left.Equals(right);
    }
}
