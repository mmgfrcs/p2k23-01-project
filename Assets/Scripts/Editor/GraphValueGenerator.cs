using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Editor
{
    public class GraphValueGenerator : EditorWindow
    {

        [SerializeField] private TreeViewState _tableState;
        [SerializeField] private MultiColumnHeaderState _headerState;
        [SerializeField] private NamedCurve[] curves;        
        
        private ValueTableView _tableView;
        private MultiColumnHeader _tableHeader;

        private SerializedObject _curveObj;
        private SerializedProperty _curveProp;
        
        [MenuItem("Window/Graph Value Generator")]
        private static void ShowWindow()
        {
            var window = GetWindow<GraphValueGenerator>();
            window.titleContent = new GUIContent("Graph Value Generator");
            window.Show();
        }
        
        void OnEnable ()
        {
            if (_tableState == null) _tableState = new TreeViewState();
            if (_headerState == null)
                _headerState = new MultiColumnHeaderState(new []
                {
                    new MultiColumnHeaderState.Column()
                });
            
            _tableHeader = new MultiColumnHeader(_headerState);
            _tableView = new ValueTableView(_tableState, _tableHeader);
            
            _curveObj = new SerializedObject(this);
            _curveObj.Update();
            _curveProp = _curveObj.FindProperty("curves");
            
        }

        private void OnGUI()
        {
            EditorGUILayout.PropertyField(_curveProp, true);
            _curveObj.ApplyModifiedProperties();
            
            EditorGUILayout.Space();
            float waves = 0;
            if (curves == null || curves.Length == 0) EditorGUILayout.LabelField("Waves", "0");
            else
            {
                var keyframes = curves.SelectMany(x => x.curve.keys).ToArray();
                if(keyframes.Length != 0) waves = keyframes.Max(x => x.time);
                EditorGUILayout.LabelField("Waves", $"{waves+1}");
            }

            
            EditorGUILayout.Space();
            if (GUILayout.Button("Generate"))
            {
                var path = EditorUtility.SaveFilePanel("Save Spawn Timings", Application.dataPath, "Genval", "csv");
                if (path.Length == 0) return;

                StringBuilder sb = new StringBuilder();
                sb.AppendLine(string.Join(",", curves.Select(x => x.name).Prepend("Wave")));
                
                for (float i = 0; i <= waves; i++)
                {
                    sb.AppendLine(string.Join(",", curves.Select(x => Mathf.Round(x.curve.Evaluate(i))).Prepend(i+1)));
                }

                File.WriteAllText(path, sb.ToString());
                Debug.Log($"[Values Generator] Saved values to {path}");
            }
            
            if (GUILayout.Button("Preview"))
            {
                _tableState = new TreeViewState();
                _headerState = new MultiColumnHeaderState(curves.Select(x => new MultiColumnHeaderState.Column()
                {
                    headerContent = new GUIContent(x.name),
                    canSort = false,
                    allowToggleVisibility = false,
                    autoResize = true
                }).Prepend(new MultiColumnHeaderState.Column()
                {
                    headerContent = new GUIContent("Wave"),
                    canSort = false,
                    allowToggleVisibility = false,
                    autoResize = true
                }).ToArray());
                
                _tableHeader = new MultiColumnHeader(_headerState);

                List<ValueTableItem> valList = new List<ValueTableItem>();
                for (float i = 0; i <= waves; i++)
                {
                    valList.Add(new ValueTableItem() { value = curves.Select(x => Mathf.Round(x.curve.Evaluate(i))).ToArray()});
                }
                
                _tableView = new ValueTableView(_tableState, _tableHeader, valList.ToArray());
            }
            
            
            _tableView.OnGUI(EditorGUILayout.GetControlRect(false, 160));
        }
    }
    
    class ValueTableView : TreeView
    {
        private readonly List<ValueTableItem> rows;
        
        public ValueTableView(TreeViewState state, MultiColumnHeader multiColumnHeader, params ValueTableItem[] items) : base(state, multiColumnHeader)
        {
            // Custom setup
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            customFoldoutYOffset = (rowHeight - EditorGUIUtility.singleLineHeight) * 0.5f; 
            extraSpaceBeforeIconAndLabel = rowHeight;
            rows = new List<ValueTableItem>(items);
            Reload();
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            return base.BuildRows(root);
        }

        protected override TreeViewItem BuildRoot()
        {
            TreeViewItem root = new TreeViewItem(0, -1, "");
            for (int i = 0; i < rows.Count; i++)
            {
                root.AddChild(new TreeViewItem<ValueTableItem>(i+1, 0, "Test", rows[i]));
            }
            return root;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = (TreeViewItem<ValueTableItem>) args.item;
        
            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                // Center the cell rect vertically using EditorGUIUtility.singleLineHeight.
                // This makes it easier to place controls and icons in the cells.
                var cellRect = args.GetCellRect(i);
                CenterRectUsingSingleLineHeight(ref cellRect);
                
                if(args.GetColumn(i) == 0) GUI.Label(cellRect, $"{args.row+1}");
                else GUI.Label(cellRect, $"{item.data.value[args.GetColumn(i)-1]}");
            }
        }
    }

    [Serializable]
    struct ValueTableItem
    {
        public float[] value;
    }
    
    class TreeViewItem<T> : TreeViewItem
    {
        public T data { get; set; }

        public TreeViewItem (int id, int depth, string displayName, T data) : base (id, depth, displayName)
        {
            this.data = data;
        }
    }

    [Serializable]
     struct NamedCurve
    {
        public string name;
        public AnimationCurve curve;
    }
}