using System.Collections.Generic;
using System.IO;
using System.Linq;
using AdInfinitum.Database;
using AdInfinitum.Entities;
using AdInfinitum.Utilities;
using UnityEditor;
using UnityEngine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace AdInfinitum.Editor
{
    [CustomEditor(typeof(MapConfig))]
    public class MapConfigEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            MapConfig confObj = (MapConfig)target;
            base.OnInspectorGUI();
            
            if (GUILayout.Button("Save Spawn Timings To File"))
            {
                var path = EditorUtility.SaveFilePanel("Save Spawn Timings", Application.dataPath, confObj.name, "yml");
                if (path.Length == 0) return;
                var yamlSerializer = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();

                var serialized = yamlSerializer.Serialize(confObj.spawnTimings.Select(x => x.ToYML()).ToList());
                File.WriteAllText(path, serialized);
                Debug.Log($"[Save Spawn Timings] Saved timings to {path}");
            }
            
            if (GUILayout.Button("Load Spawn Timings From File"))
            {
                var path = EditorUtility.OpenFilePanel("Load Spawn Timings", Application.dataPath, "yml");
                if (path.Length == 0) return;
                var yamlDeserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();

                var serialized = File.ReadAllText(path);
                var deserialized = yamlDeserializer.Deserialize<List<SpawnTimingYML>>(serialized);
                confObj.spawnTimings.Clear();
                for (int i = 0; i < deserialized.Count; i++)
                {
                    var assetPaths = AssetDatabase.FindAssets($"{deserialized[i].enemyPrefab} l:Enemy");
                    if (assetPaths.Length == 0)
                    {
                        Debug.LogError($"[Load Spawn Timings] ({path}) Cannot load prefab for enemy {deserialized[i].enemyPrefab} at timing {i+1}");
                        continue;
                    }

                    var st = new SpawnTiming()
                    {
                        enemyPrefab =
                            AssetDatabase.LoadAssetAtPath<Enemy>(AssetDatabase.GUIDToAssetPath(assetPaths[0])),
                        amount = deserialized[i].amount,
                        delay = deserialized[i].delay,
                        health = deserialized[i].health,
                        waitTimeMultiplier = deserialized[i].waitTimeMultiplier
                    };
                    
                    confObj.spawnTimings.Add(st);
                }
                
                EditorUtility.SetDirty(target);
                
                Debug.Log($"[Load Spawn Timings] Loaded timings from {path}");
            }
        }
    }
}