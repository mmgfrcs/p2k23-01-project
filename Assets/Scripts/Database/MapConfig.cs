using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpawnConfig", menuName = "Spawn Configuration", order = 0)]
public class MapConfig : ScriptableObject
{
    public List<SpawnTiming> spawnTimings;
}
