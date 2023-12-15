using System.Globalization;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class MapConfigGenerator : EditorWindow
    {
        [SerializeField] private AnimationCurve hpCurve = new AnimationCurve(new Keyframe(1,0), new Keyframe(2,0));
        [SerializeField] private AnimationCurve amountCurve = new AnimationCurve(new Keyframe(1,0), new Keyframe(2,0));
        [SerializeField] private AnimationCurve waitTimeCurve = new AnimationCurve(new Keyframe(1,0), new Keyframe(2,0));
        [SerializeField] private AnimationCurve delayCurve = new AnimationCurve(new Keyframe(1,0), new Keyframe(2,0));
        [SerializeField] private EnemyType defaultEnemy;
        [SerializeField] private int interval = 8, cooldown = 4;
        [MenuItem("Window/Map Config Generator")]
        private static void ShowWindow()
        {
            var window = GetWindow<MapConfigGenerator>();
            window.titleContent = new GUIContent("Map Config Generator");
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.PrefixLabel("HP Curve");
            hpCurve = EditorGUILayout.CurveField(hpCurve, GUILayout.Height(32));
            EditorGUILayout.PrefixLabel("Amount Curve");
            amountCurve = EditorGUILayout.CurveField(amountCurve, GUILayout.Height(32));
            EditorGUILayout.PrefixLabel("Wait Time Curve");
            waitTimeCurve = EditorGUILayout.CurveField(waitTimeCurve, GUILayout.Height(32));
            EditorGUILayout.PrefixLabel("Delay Curve");
            delayCurve = EditorGUILayout.CurveField(delayCurve, GUILayout.Height(32));
            defaultEnemy = (EnemyType)EditorGUILayout.EnumPopup("Normal Enemy Type", defaultEnemy);
            interval = EditorGUILayout.IntField("Interval", interval);
            cooldown = EditorGUILayout.IntField("Cooldown", cooldown);
            //EditorGUILayout.IntField("Normal Enemy", defaultEnemy);
            //EditorGUILayout.IntField("Normal Enemy", defaultEnemy);
            EditorGUILayout.Space();
            var waves = Mathf.Min(hpCurve.keys.Max(x => x.time), amountCurve.keys.Max(x => x.time),
                waitTimeCurve.keys.Max(x => x.time), delayCurve.keys.Max(x => x.time));
            
            EditorGUILayout.LabelField("Waves", $"{waves}");
            EditorGUILayout.LabelField("HP Curve", $"{Mathf.Round(hpCurve.Evaluate(1))} {Mathf.Round(hpCurve.Evaluate(11))} {Mathf.Round(hpCurve.Evaluate(21))} {Mathf.Round(hpCurve.Evaluate(31))}");
            EditorGUILayout.LabelField("Amount Curve", $"{Mathf.Round(amountCurve.Evaluate(1))} {Mathf.Round(amountCurve.Evaluate(11))} {Mathf.Round(amountCurve.Evaluate(21))} {Mathf.Round(amountCurve.Evaluate(31))}");
            EditorGUILayout.LabelField("Delay Curve", $"{delayCurve.Evaluate(1):N2} {delayCurve.Evaluate(11):N2} {delayCurve.Evaluate(21):N2} {delayCurve.Evaluate(31):N2}");
            EditorGUILayout.LabelField("Wait Time Curve", $"{waitTimeCurve.Evaluate(1):N2} {waitTimeCurve.Evaluate(11):N2} {waitTimeCurve.Evaluate(21):N2} {waitTimeCurve.Evaluate(31):N2}");
            if (GUILayout.Button("Generate"))
            {
                
            }
        }
    }
}