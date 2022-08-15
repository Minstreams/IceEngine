using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using IceEngine;
using IceEngine.Internal;
using static IceEditor.IceGUI;
using static IceEditor.IceGUIAuto;
using IceEditor.Framework;
using UnityEditor;

namespace IceEditor.Internal
{
    public class IceprintBox : IceEditorWindow
    {
        #region 配置
        const string GRAPH_KEY = "GraphView";
        const float MIN_SCALE = 0.4f;
        const float MAX_SCALE = 4.0f;
        const float GRID_SIZE = 32;

        Color GridColor => ThemeColor * 0.5f;
        GUIStyle StlGraphBackgroud => StlDock;
        #endregion

        #region Graph
        public static void OpenPrint(Iceprint print) => GetWindow<IceprintBox>().Graph = print;
        public Iceprint Graph
        {
            get => _graph;
            set
            {
                if (_graph != value)
                {
                    _graph = value;
                    // On Change
                    // ...
                }
            }
        }
        Iceprint _graph;
        #endregion

        #region Port
        int idDragPort;
        IceprintPort DraggingPort { get; set; }

        readonly HashSet<IceprintPort> AvailablePorts = new();

        void BeginDragPort(IceprintPort port)
        {
            DraggingPort = port;

            if (port.IsOutport)
            {
                foreach (var node in Graph.nodeList) foreach (var p in node.inports) if (Graph.IsConnectable(port, p)) AvailablePorts.Add(p);
            }
            else
            {
                foreach (var node in Graph.nodeList) foreach (var p in node.outports) if (Graph.IsConnectable(port, p)) AvailablePorts.Add(p);
            }
        }
        void EndDragPort()
        {
            DraggingPort = null;
            AvailablePorts.Clear();
        }
        #endregion

        #region Node
        HashSet<IceprintNode> selectedNodes = new();

        #endregion

        protected override void OnWindowGUI(Rect position)
        {
            using (DOCK)
            {
                Graph = _ObjectField(Graph, true, GUILayout.Width(20));

                Space();

                if (IceButton("重置视图"))
                {
                    SetFloat($"{GRAPH_KEY}_ViewScale", 1);
                    SetVector2($"{GRAPH_KEY}_ViewOffset", Vector2.zero);
                }
                if (IceButton("Test"))
                {
                    Graph.AddNode<TestNode>();
                }
            }

            {
                if (Graph == null)
                {
                    var area = GetRect(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                    using (var viewport = ViewportGrid(GRAPH_KEY, area, GRID_SIZE, 1, MIN_SCALE, MAX_SCALE, null, GridColor * 0.7f, StlGraphBackgroud, false)) { }
                    StyleBox(area, StlBackground, "Null");
                }
                else
                {
                    OnGraphGUI();
                }
            }
        }

        void OnGraphGUI()
        {
            var area = GetRect(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            using var viewport = ViewportGrid(GRAPH_KEY, area, GRID_SIZE, 1, MIN_SCALE, MAX_SCALE, null, GridColor, StlGraphBackgroud, false);

            // 强制刷新
            if (E.type == EventType.MouseMove) Repaint();

            // 快捷键
            if (E.type == EventType.KeyDown)
            {
                //if (E.keyCode == KeyCode.Delete)
                //{
                //    foreach (var node in selectedNodes)
                //    {
                //        Graph.RemoveNode(node);
                //    }
                //    selectedNodes.Clear();
                //    Repaint();
                //}
            }

            // 画node
            foreach (var node in Graph.nodeList)
            {
                var drawer = node.GetDrawer();
                bool bSelected = selectedNodes.Contains(node);

                // 背景
                var nodeRect = node.GetArea();
                StyleBox(nodeRect, bSelected ? drawer.StlGraphNodeBackgroundSelected : drawer.StlGraphNodeBackground);

                // 标题
                var titleRect = new Rect(nodeRect) { height = node.GetSizeTitle().y };
                drawer.OnGUI_Title(node, titleRect);

                // 主体
                if (!node.folded)
                {
                    var bodyRect = new Rect(node.position, node.GetSizeBody()).Move(y: node.GetSizeTitle().y);
                    drawer.OnGUI_Body(node, bodyRect);
                }


            }
        }



        #region 定制
        [MenuItem("IceEngine/Iceprint Box")]
        static void OpenWindow() => GetWindow<IceprintBox>();

        protected override void OnEnable()
        {
            base.OnEnable();
            wantsMouseMove = true;
        }
        #endregion

        #region Debug
        protected override void OnDebugGUI(Rect position)
        {
            using (GROUP)
            {
                Label("Console:");
                Label(GetString("Console"));
            }
            base.OnDebugGUI(position);
        }
        int? baseStack = null;
        void OnLog(string log)
        {
            string curLog = GetString("Console");

            string prefix = "";
            System.Diagnostics.StackTrace st = new();
            int fc = st.FrameCount;
            if (baseStack == null)
            {
                baseStack = fc;
                log = log.Replace("\n", "");
            }
            else
            {
                int indent = fc - baseStack.Value;
                if (indent > 0)
                {
                    for (int i = 0; i < indent; ++i) prefix += "|       ".Color(GetColor(i));
                    log = log.Replace("\n", $"\n{prefix}");
                }
            }

            Color GetColor(int indent)
            {
                return (indent % 3) switch
                {
                    0 => Color.black,
                    1 => Color.gray,
                    2 => new Color(0.4f, 0.2f, 0.1f),
                    _ => Color.red,
                };
            }

            curLog += log;

            SetString("Console", curLog);
        }
        IceBinaryUtility.LogScope LOG
        {
            get
            {
                SetString("Console", "");
                return new IceBinaryUtility.LogScope(OnLog);
            }
        }
        #endregion
    }
}
