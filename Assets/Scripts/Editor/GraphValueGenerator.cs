using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class GraphValueGenerator : EditorWindow
    {
        [SerializeField] private AnimationCurve curve = new AnimationCurve(new Keyframe(1,0), new Keyframe(2,0));
        [MenuItem("Window/Graph Value Generator")]
        private static void ShowWindow()
        {
            var window = GetWindow<GraphValueGenerator>();
            window.titleContent = new GUIContent("Map Config Generator");
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.PrefixLabel("Curve");
            curve = EditorGUILayout.CurveField(curve, GUILayout.Height(32));
            //EditorGUILayout.IntField("Normal Enemy", defaultEnemy);
            //EditorGUILayout.IntField("Normal Enemy", defaultEnemy);
            EditorGUILayout.Space();
            var waves = curve.keys.Max(x => x.time);
            
            EditorGUILayout.LabelField("Waves", $"{waves+1}");
            GUIStyle textStyle = EditorStyles.label;
            textStyle.wordWrap = true;
            EditorGUILayout.LabelField("Values", EvaluateGraph(waves, f => curve.Evaluate(f)), textStyle);
            
            EditorGUILayout.Space();
            if (GUILayout.Button("Generate"))
            {
                var path = EditorUtility.SaveFilePanel("Save Spawn Timings", Application.dataPath, "Genval", "csv");
                if (path.Length == 0) return;

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Wave,Value");
                for (float i = 0; i <= waves; i++)
                {
                    sb.AppendLine($"{i + 1},{Math.Round(curve.Evaluate(i))}");
                }

                File.WriteAllText(path, sb.ToString());
                Debug.Log($"[Values Generator] Saved values to {path}");
            }
        }

        private string EvaluateGraph(float time, Func<float, float> valFunc, float step = 1, string sep = " ")
        {
            List<float> vals = new List<float>();
            for (float i = 0; i <= time; i+=step)
            {
                vals.Add(Mathf.Round(valFunc.Invoke(i)));
            }
            return string.Join(sep, vals);
        }
    }
}