using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using IceEngine;
using IceEngine.Framework;
using IceEngine.Internal;
using IceEngine.IceprintNodes;
using static IceEditor.IceGUI;
using static IceEditor.IceGUIAuto;
using IceEditor.Framework;
using UnityEditor;
using System;

namespace IceEditor.Internal
{
    public class IceprintBox : IceEditorWindow
    {
        #region 配置
        const string GRAPH_KEY = "GraphView";

        public static EditorSettingIceprintBox Setting => EditorSettingIceprintBox.Setting;
        GUIStyle StlGraphBackgroud => StlDock;

        string GetParamTypeName(Type paramType)
        {
            if (paramType is null) return "void";
            if (paramType == typeof(int)) return "int";
            if (paramType == typeof(float)) return "float";
            if (paramType == typeof(string)) return "string";
            return paramType.Name;
        }
        Color GetColor(Type paramType)
        {
            if (paramType == typeof(int)) return Color.cyan;
            if (paramType == typeof(float)) return new Color(1.0f, 0.6f, 0.2f);
            if (paramType == typeof(string)) return new Color(1.0f, 0.7f, 0.1f);
            return Color.white;
        }
        #endregion

        #region Graph
        byte[] buffer = null;
        public static void OpenPrint(Iceprint print) => GetWindow<IceprintBox>().Graph = print;
        public Iceprint Graph
        {
            get => _graph;
            set
            {
                if (_graph != value)
                {
                    // On Change
                    selectedNodes.Clear();
                    buffer = null;
                    undoList.Clear();
                    redoList.Clear();
                    if (value != null)
                    {
                        if (!EditorApplication.isPlaying)
                        {
                            value.Deserialize();
                        }
                        buffer = value.graphData;
                        if (buffer is null || buffer.Length == 0)
                        {
                            using (LOG)
                            {
                                buffer = value.Serialize();
                            }
                        }
                    }
                }
                _graph = value;
            }
        }
        Iceprint _graph;

        #region Undo & Redo
        readonly LinkedList<byte[]> undoList = new();
        readonly LinkedList<byte[]> redoList = new();

        void RecordForUndo()
        {
            Repaint();
            using (LOG)
            {
                byte[] cur = Graph.Serialize();
                if (Equals(cur, buffer))
                {
                    buffer = cur;
                    return;
                }

                static bool Equals(byte[] b1, byte[] b2)
                {
                    if (b1.Length != b2.Length) return false;
                    for (int i = 0; i < b1.Length; ++i) if (b1[i] != b2[i]) return false;
                    return true;
                }

                if (undoList.Count >= Setting.maxUndoCount) undoList.RemoveFirst();
                undoList.AddLast(IceBinaryUtility.GetDiff(buffer, cur));
                redoList.Clear();
                buffer = cur;
            }
        }
        void Undo()
        {
            if (undoList.Count == 0) return;
            var diff = undoList.Last.Value;
            undoList.RemoveLast();
            buffer = IceBinaryUtility.ReverseDiff(buffer, diff);
            redoList.AddLast(diff);
            Graph.Deserialize(buffer);

            GUIUtility.keyboardControl = 0;
        }
        void Redo()
        {
            if (redoList.Count == 0) return;
            var diff = redoList.Last.Value;
            redoList.RemoveLast();
            buffer = IceBinaryUtility.ApplyDiff(buffer, diff);
            undoList.AddLast(diff);
            Graph.Deserialize(buffer);

            GUIUtility.keyboardControl = 0;
        }
        #endregion

        #region Copy & Paste
        byte[] copyBuffer = null;
        void CopyBuffer()
        {
            copyBuffer = buffer;
        }
        void PasteBuffer()
        {
            Graph.Deserialize(copyBuffer);
            RecordForUndo();
        }
        #endregion

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

        Color GetColor(IceprintPort port)
        {
            Color c = Color.white;
            for (int i = 0; i < port.ParamsList.Count; ++i)
            {
                c = Color.LerpUnclamped(GetColor(port.ParamsList[i]), c, i / ((float)(i + 1)));
            }
            return c;
        }
        #endregion

        #region Node

        #region Runtime Const
        HashSet<Type> _runtimeConstNodeTypeSet = null;
        HashSet<Type> RuntimeConstNodeTypeSet
        {
            get
            {
                if (_runtimeConstNodeTypeSet is null)
                {
                    _runtimeConstNodeTypeSet = new();
                    Type runtimeConstType = typeof(RuntimeConstAttribute);
                    foreach (var node in TypeCache.GetTypesDerivedFrom<IceprintNode>())
                    {
                        if (node.GetCustomAttributes(runtimeConstType, false).Length > 0)
                        {
                            _runtimeConstNodeTypeSet.Add(node);
                        }
                    }
                }
                return _runtimeConstNodeTypeSet;
            }
        }
        public bool IsRuntimeConst(Type nodeType) => Setting.allowRuntimeConst && RuntimeConstNodeTypeSet.Contains(nodeType);
        public bool IsRuntimeConst(IceprintNode node) => Setting.allowRuntimeConst && RuntimeConstNodeTypeSet.Contains(node.GetType());
        #endregion

        #region Create Node Menu
        List<(GUIContent path, Type node)> _nodeMenu = null;
        List<(GUIContent path, Type node)> NodeMenu
        {
            get
            {
                if (_nodeMenu is null)
                {
                    _nodeMenu = new();
                    Type menuItemType = typeof(IceprintMenuItemAttribute);
                    foreach (var node in TypeCache.GetTypesDerivedFrom<IceprintNode>())
                    {
                        foreach (IceprintMenuItemAttribute item in node.GetCustomAttributes(menuItemType, false))
                        {
                            _nodeMenu.Add((new GUIContent(item.Path), node));
                        }
                    }
                }
                return _nodeMenu;
            }
        }
        void ShowCreateNodeMenu()
        {
            SetBool("isCreateNodeMenuOn", true);
            GenericMenu gm = new GenericMenu();
            gm.allowDuplicateNames = true;
            Vector2 pos = E.mousePosition;
            foreach (var nm in NodeMenu)
            {
                if (EditorApplication.isPlaying && IsRuntimeConst(nm.node))
                {
                    gm.AddDisabledItem(nm.path);
                }
                else
                {
                    gm.AddItem(nm.path, false, () =>
                    {
                        var node = Graph.AddNode(nm.node, pos);
                        selectedNodes.Clear();
                        selectedNodes.Add(node);
                        RecordForUndo();
                    });
                }
            }

            foreach (var g in Graph.gameObject.scene.GetRootGameObjects())
            {
                var nodeComps = g.GetComponentsInChildren<IceprintNodeComponent>();
                foreach (var nodeComp in nodeComps)
                {
                    gm.AddItem(new GUIContent(GetMenuPath(nodeComp)), false, () =>
                    {
                        var node = Graph.AddNode(new NodeMonoBehaviour()
                        {
                            targetType = nodeComp.GetType(),
                        }, pos) as NodeMonoBehaviour;

                        node.target.Value = nodeComp;

                        selectedNodes.Clear();
                        selectedNodes.Add(node);
                        RecordForUndo();
                    });
                }
            }

            string GetMenuPath(IceprintNodeComponent node) => $"Components/{node.GetPath()}";

            gm.ShowAsContext();
        }
        #endregion

        #region Selection & Drag
        int idSelectNode;
        int idDragNode;
        readonly HashSet<IceprintNode> selectedNodes = new();

        void DeleteSelectedNodes()
        {
            foreach (var node in selectedNodes)
            {
                if (EditorApplication.isPlaying && IsRuntimeConst(node)) continue;
                Graph.RemoveNode(node);
            }
            selectedNodes.Clear();
            RecordForUndo();
        }
        #endregion

        #endregion

        #region Utility
        public void ResetGraphView()
        {
            if (Graph != null)
            {
                Vector2 offset = Vector2.zero;
                Vector2 min = Vector2.positiveInfinity;
                Vector2 max = Vector2.negativeInfinity;
                foreach (var node in Graph.nodeList)
                {
                    var pos = node.GetArea().center;
                    offset += pos;
                    min = Vector2.Min(min, pos);
                    max = Vector2.Max(max, pos);
                }
                offset /= Graph.nodeList.Count;
                offset = offset.Snap(Setting.gridSize);
                if (offset != Vector2.zero)
                {
                    foreach (var node in Graph.nodeList)
                    {
                        node.position -= offset;
                    }
                    RecordForUndo();
                }
            }

            SetFloat($"{GRAPH_KEY}_ViewScale", 1);
            SetVector2($"{GRAPH_KEY}_ViewOffset", Vector2.zero);
        }
        #endregion

        protected override void OnWindowGUI(Rect position)
        {
            using (DOCK)
            {
                Graph = _ObjectField(Graph, true, GUILayout.Width(20));
                if (IceButton("<", undoList.Count > 0) || (E.type == EventType.KeyDown && E.keyCode == KeyCode.Z && E.control))
                {
                    Undo();
                    E.Use();
                }
                if (IceButton(">", redoList.Count > 0) || (E.type == EventType.KeyDown && E.keyCode == KeyCode.Y && E.control))
                {
                    Redo();
                    E.Use();
                }
                if (IceButton("Copy")) CopyBuffer();
                if (IceButton("Paste")) PasteBuffer();
                Space();

                if (selectedNodes.Count > 1)
                {
                    // 批量操作
                    Label($"当前选择{selectedNodes.Count}个Node");
                    if (IceButton("删除") || (E.type == EventType.KeyDown && E.keyCode == KeyCode.Delete)) DeleteSelectedNodes();
                }
                else if (selectedNodes.Count == 1)
                {
                    // 单个操作
                    var itr = selectedNodes.GetEnumerator();
                    itr.MoveNext();
                    Label($"当前选择 {itr.Current.GetDisplayName()}");
                    if (IceButton("删除") || (E.type == EventType.KeyDown && E.keyCode == KeyCode.Delete)) DeleteSelectedNodes();
                }
                else
                {
                    // 未选择任何
                    if (IceButton("重置视图"))
                    {
                        ResetGraphView();
                    }
                }

                if (EditorApplication.isPlaying)
                {
                    if (Graph != null)
                    {
                        if (!Setting.allowRuntimeConst && IceButton("刷新"))
                        {
                            Graph.ReloadGraph();
                        }
                    }
                }
            }

            {
                string title = "Iceprint Box";
                if (Graph == null)
                {
                    var area = GetRect(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                    using (var viewport = ViewportGrid(GRAPH_KEY, area, Setting.gridSize, 1, Setting.minScale, Setting.maxScale, null, Setting.gridColor * 0.7f, StlGraphBackgroud, false)) { }
                    StyleBox(area, StlBackground, "Null");
                }
                else
                {
                    OnGraphGUI();
                    if (EditorUtility.IsDirty(Graph)) title = "Iceprint Box*";
                }

                if (E.type == EventType.Repaint)
                {
                    if (_title != title)
                    {
                        _title = title;
                        RefreshTitleContent();
                    }
                }
            }
        }
        void OnGraphGUI()
        {
            var area = GetRect(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            using var viewport = ViewportGrid(GRAPH_KEY, area, Setting.gridSize, 1, Setting.minScale, Setting.maxScale, null, Setting.gridColor, StlGraphBackgroud, false);

            // 强制刷新
            if (E.type == EventType.MouseMove) Repaint();

            // Control
            idSelectNode = GetControlID();
            idDragNode = GetControlID();
            idDragPort = GetControlID();
            switch (E.type)
            {
                case EventType.MouseUp:
                    if (E.button == 0)
                    {
                        if (GUIHotControl == idSelectNode)
                        {
                            GUIHotControl = 0;
                            E.Use();
                        }
                        else if (GUIHotControl == idDragNode)
                        {
                            GUIHotControl = 0;
                            if (_cache_pos != E.mousePosition)
                            {
                                RecordForUndo();
                            }
                            E.Use();
                        }
                    }
                    else if (GUIHotControl == idSelectNode || GUIHotControl == idDragNode || GUIHotControl == idDragPort)
                    {
                        E.Use();
                    }
                    break;
                case EventType.MouseDrag:
                    if (GUIHotControl == idSelectNode)
                    {
                        // 选择逻辑
                        selectedNodes.Clear();
                        Rect selectionRect = _cache_drag.RangeRect(E.mousePosition);
                        foreach (var node in Graph.nodeList)
                        {
                            var nodeArea = node.GetArea();

                            if (Setting.mustContainOnSelect ? selectionRect.Contains(nodeArea) : nodeArea.Overlaps(selectionRect)) selectedNodes.Add(node);
                        }
                        E.Use();
                    }
                    else if (GUIHotControl == idDragNode)
                    {
                        // 拖动逻辑
                        var pos = E.mousePosition;

                        if (E.shift)
                        {
                            // 平移操作
                            var offset = pos - _cache_pos;
                            if (Mathf.Abs(offset.x) > Mathf.Abs(offset.y))
                            {
                                // 沿x轴平移
                                pos.y = _cache_pos.y;
                            }
                            else
                            {
                                // 沿y轴平移
                                pos.x = _cache_pos.x;
                            }
                        }
                        if (E.control)
                        {
                            // 网格吸附操作
                            pos = (pos - _cache_offset).Snap(Setting.gridSize) + _cache_offset;
                        }

                        foreach (var node in selectedNodes)
                        {
                            node.position += pos - _cache_drag;
                        }
                        _cache_drag = pos;

                        E.Use();
                    }
                    else if (GUIHotControl == idDragPort)
                    {
                        E.Use();
                    }
                    break;
            }

            // Node
            foreach (var node in Graph.nodeList)
            {
                var drawer = node.GetDrawer();
                bool bSelected = selectedNodes.Contains(node);

                // 背景
                var nodeRect = node.GetArea();
                if (E.type == EventType.Repaint)
                {
                    if (bSelected)
                    {
                        if (GUIHotControl == idDragNode)
                        {
                            // 在原始位置画一个残影
                            var holderRect = nodeRect.Move(_cache_pos - _cache_drag);
                            using (GUIColor(Color.white * 0.6f)) StyleBox(holderRect, drawer.StlGraphNodeBackground);
                        }
                        StyleBox(nodeRect, StlSelectedBox);
                    }
                    StyleBox(nodeRect, drawer.StlGraphNodeBackground);
                }

                using (GUICHECK)
                {
                    // 标题
                    var titleRect = new Rect(nodeRect) { height = node.GetSizeTitle().y };
                    drawer.OnGUI_Title(node, titleRect);

                    // 主体
                    if (!node.folded)
                    {
                        var bodyRect = new Rect(node.position, node.GetSizeBody()).Move(y: node.GetSizeTitle().y);
                        drawer.OnGUI_Body(node, bodyRect);
                    }

                    if (GUIChanged)
                    {
                        RecordForUndo();
                    }
                }

                // 拖动
                if (nodeRect.Contains(E.mousePosition) && E.type == EventType.MouseDown && E.button == 0)
                {
                    // 拖动
                    double t = EditorApplication.timeSinceStartup;
                    if (t - _cache_time < 0.2)
                    {
                        // 双击折叠
                        _cache_time = 0;
                        node.folded = !node.folded;
                        RecordForUndo();
                        E.Use();
                    }
                    else
                    {
                        _cache_time = t;

                        // 开始拖动
                        if (GUIHotControl == 0)
                        {
                            if (!bSelected)
                            {
                                if (!E.control && !E.shift)
                                {
                                    selectedNodes.Clear();
                                    drawer.OnSelect(node);
                                }
                                selectedNodes.Add(node);
                            }

                            GUIHotControl = idDragNode;
                            _cache_drag = E.mousePosition;
                            _cache_pos = E.mousePosition;
                            _cache_offset = E.mousePosition - node.position;
                            E.Use();
                        }
                    }
                }
            }

            // Port
            if (E.type == EventType.Repaint && GUIHotControl == idDragPort)
            {
                var pos = DraggingPort.GetPos();
                var tagent = DraggingPort.GetTangent();
                var color = GetColor(DraggingPort);
                IceGUIUtility.DrawPortLine(pos, E.mousePosition, tagent, color, color);
                IceGUIUtility.DrawPortLine(E.mousePosition, pos, -tagent, color, color);
            }
            foreach (var node in Graph.nodeList)
            {
                foreach (var port in node.inports) DrawLine(port);
                foreach (var port in node.outports) DrawLine(port);
            }
            void DrawLine(IceprintPort port)
            {
                if (E.type == EventType.Repaint && port.IsConnected)
                {
                    Vector2 pos = port.GetPos();
                    var color = GetColor(port);

                    var tagent = port.GetTangent();
                    if (port is IceprintInport pin)
                    {
                        foreach (var pp in pin.connectedPorts) IceGUIUtility.DrawPortLine(pos, pp.GetPos(), tagent, color, GetColor(pp));
                    }
                    else if (port is IceprintOutport pout)
                    {
                        foreach (var pp in pout.connectedPorts) IceGUIUtility.DrawPortLine(pos, pp.port.GetPos(), tagent, color, GetColor(pp.port));
                    }
                }
            }
            foreach (var node in Graph.nodeList)
            {
                if (node.folded) continue;
                foreach (var port in node.inports) OnGUI_Port(port);
                foreach (var port in node.outports) OnGUI_Port(port);
            }
            void OnGUI_Port(IceprintPort port)
            {
                Vector2 pos = port.GetPos();
                var color = GetColor(port);

                Rect rPort = pos.ExpandToRect(IceGUIUtility.PORT_RADIUS);
                bool bHover = rPort.Contains(E.mousePosition);

                switch (E.type)
                {
                    case EventType.MouseDown:
                        if (bHover)
                        {
                            if (GUIHotControl == 0 && E.button == 0)
                            {
                                // Begin drag Control
                                _cache_offset = rPort.center;
                                GUIHotControl = idDragPort;
                                if (!port.isMultiple && port.IsConnected)
                                {
                                    port.DisconnectAll();
                                    RecordForUndo();
                                }
                                BeginDragPort(port);
                                E.Use();
                            }
                            else if (E.button == 1)
                            {
                                // Disconnect port
                                if (port.IsConnected)
                                {
                                    port.DisconnectAll();
                                    RecordForUndo();
                                }
                                E.Use();
                            }
                        }
                        break;
                    case EventType.MouseUp:
                        if (bHover && GUIHotControl == idDragPort && E.button == 0 && AvailablePorts.Contains(port))
                        {
                            // Connect ports
                            DraggingPort.ConnectTo(port);
                            RecordForUndo();
                            GUIHotControl = 0;
                            EndDragPort();
                            E.Use();
                        }
                        break;
                    case EventType.Repaint:
                        // Port
                        {
                            if (GUIHotControl == idDragPort)
                            {
                                // 拖拽状态
                                if (DraggingPort == port)
                                {
                                    // 被拖拽的 Port
                                    DiscSolid(0.15f, color);
                                }
                                else
                                {
                                    // 其他 Port
                                    if (port.IsConnected)
                                    {
                                        // 连接状态
                                        DiscSolid(0.3f, color);
                                    }

                                    if (AvailablePorts.Contains(port))
                                    {
                                        // 备选状态
                                        DiscWire(0.2f, color * 0.8f);
                                        DiscWire(0.8f, color * 0.7f);

                                        if (bHover)
                                        {
                                            // hover
                                            DiscWire(0.8f, IceGUIUtility.CurrentThemeColor);
                                        }
                                        else
                                        {
                                            // 未hover
                                            DiscWire(0.8f, IceGUIUtility.CurrentThemeColor * 0.7f);
                                        }
                                    }
                                    else
                                    {
                                        // 非备选直接隐藏
                                    }
                                }
                            }
                            else
                            {
                                if (port.IsConnected)
                                {
                                    // 连接状态
                                    DiscSolid(0.3f, color);
                                }
                                else
                                {
                                    // 普通
                                    DiscWire(0.3f, color * 0.8f);
                                }

                                if (bHover)
                                {
                                    // hover
                                    DiscWire(0.6f, color);
                                }
                            }


                            // 画 Port 内圈
                            void DiscWire(float radius, Color color)
                            {
                                radius *= IceGUIUtility.PORT_RADIUS;
                                using (HandlesColor(color)) Handles.DrawWireDisc(pos, Vector3.forward, radius);
                                // 柔化边缘
                                color.a *= 0.4f;
                                using (HandlesColor(color))
                                {
                                    float off = 0.3f / GUI.matrix[0];
                                    Handles.DrawWireDisc(pos, Vector3.forward, radius + off);
                                    Handles.DrawWireDisc(pos, Vector3.forward, radius - off);
                                }
                            }
                            // 画 Port
                            void DiscSolid(float radius, Color color)
                            {
                                radius *= IceGUIUtility.PORT_RADIUS;
                                using (HandlesColor(color)) Handles.DrawSolidDisc(pos, Vector3.forward, radius);
                                // 柔化边缘
                                color.a *= 0.4f;
                                using (HandlesColor(color)) Handles.DrawWireDisc(pos, Vector3.forward, radius + 0.3f / GUI.matrix[0]);
                            }
                        }

                        // Text
                        {
                            if (GUIHotControl == 0)
                            {
                                if (bHover) DrawTextWithType(false);
                                else DrawText();
                            }
                            else if (GUIHotControl == idDragPort)
                            {
                                if (port == DraggingPort) DrawTextWithType(true);
                                else if (AvailablePorts.Contains(port)) DrawTextWithType(bHover);
                                else DrawText(false);
                            }
                            else DrawText(false);

                            void DrawText(bool? hover = null)
                            {
                                Vector2 sText = StlGraphPortName.CalcSize(TempContent(port.name));
                                Rect rText = new(port.IsOutport ? rPort.xMax + StlGraphPortName.margin.left : rPort.x - sText.x - StlGraphPortName.margin.right, rPort.y + 0.5f * (rPort.height - sText.y), sText.x, sText.y);
                                StyleBox(rText, StlGraphPortName, port.name, isHover: hover);
                            }

                            void DrawTextWithType(bool focus)
                            {
                                string tType = port.ParamsList.Count == 0 ? GetParamTypeName(null) : "";
                                for (int i = 0; i < port.ParamsList.Count; ++i)
                                {
                                    var param = port.ParamsList[i];
                                    tType += GetParamTypeName(param).Color(GetColor(param));
                                    if (i < port.ParamsList.Count - 1) tType += ", ";
                                }

                                Vector2 sType = StlGraphPortLabel.CalcSize(TempContent(tType));
                                Rect rType = new(port.IsOutport ? rPort.x - sType.x - StlGraphPortLabel.margin.right : rPort.xMax + StlGraphPortLabel.margin.left, rPort.y + 0.5f * (rPort.height - sType.y), sType.x, sType.y);
                                StyleBox(rType, StlGraphPortLabel, tType, hasKeyboardFocus: focus);

                                DrawText(true);
                            }
                        }
                        break;
                }
            }

            // 结束拖拽
            if (E.type == EventType.MouseUp && GUIHotControl == idDragPort && E.button == 0)
            {
                GUIHotControl = 0;
                EndDragPort();
                E.Use();
            }

            // 画选择框
            if (E.type == EventType.Repaint && GUIHotControl == idSelectNode)
            {
                Vector2 p0 = E.mousePosition;
                Vector2 p1 = new Vector2(_cache_drag.x, p0.y);
                Vector2 p2 = new Vector2(p0.x, _cache_drag.y);
                Handles.DrawLine(_cache_drag, p1);
                Handles.DrawLine(_cache_drag, p2);
                Handles.DrawLine(p0, p1);
                Handles.DrawLine(p0, p2);
            }

            // 内部鼠标事件
            if (viewport.ClipRect.Contains(E.mousePosition))
            {
                // 右键菜单
                switch (E.type)
                {
                    case EventType.MouseDown:
                        if (E.button == 0 && GUIHotControl == 0)
                        {
                            // 开始选择
                            GUIHotControl = idSelectNode;
                            _cache_drag = E.mousePosition;
                            selectedNodes.Clear();
                            GUIUtility.keyboardControl = 0;
                            Repaint();
                            E.Use();
                        }
                        if (E.button == 1)
                        {
                            _cache_click = E.mousePosition;
                        }
                        break;
                    case EventType.MouseUp:
                        if (E.button == 1 && E.mousePosition == _cache_click)
                        {
                            // OnClick
                            ShowCreateNodeMenu();
                            GUIHotControl = 0;
                            E.Use();
                        }
                        break;
                }
            }
        }

        #region 定制
        public static IceprintBox Instance { get; private set; } = null;

        [MenuItem("IceEngine/Iceprint Box", false, 20)]
        public static void OpenWindow() => GetWindow<IceprintBox>();
        protected override bool HasScrollScopeOnWindowGUI => false;
        protected override Color DefaultThemeColor => Setting.themeColor;
        protected override string Title => _title ??= "Iceprint Box";

        string _title = null;

        protected override void OnEnable()
        {
            base.OnEnable();
            wantsMouseMove = true;

            if (Graph != null) Graph.Deserialize(buffer);

            EditorApplication.playModeStateChanged -= OnPlayModeChange;
            EditorApplication.playModeStateChanged += OnPlayModeChange;

            Instance = this;
        }
        void OnDisable()
        {
            Instance = null;

            EditorApplication.playModeStateChanged -= OnPlayModeChange;
        }

        void OnPlayModeChange(PlayModeStateChange mode)
        {
            Graph = null;
        }
        #endregion

        #region Debug
        protected override void OnDebugGUI(Rect position)
        {
            using (DOCK)
            {
                if (IceButton("Serialize"))
                {
                    using (LOG)
                    {
                        buffer = Graph.Serialize();
                    }
                }
                if (IceButton("Deserialize")) Graph.Deserialize();
                Space();
                if (IceButton("ClearUndo"))
                {
                    undoList.Clear();
                    redoList.Clear();
                }
                if (IceButton("刷新"))
                {
                    Graph.ReloadGraph();
                }
            }
            using (GROUP) using (SectionFolder("Buffer", changeWidth: false))
            {
                if (buffer != null)
                {
                    Label(buffer.Hex());
                }
                Header($"Undo[{undoList.Count}]");
                foreach (var bts in undoList)
                {
                    Label(bts.Hex());
                }
                Header($"Redo[{redoList.Count}]");
                foreach (var bts in redoList)
                {
                    Label(bts.Hex());
                }
            }
            using (GROUP) using (SectionFolder("Console", changeWidth: false))
            {
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
            if (baseStack is null)
            {
                baseStack = fc;
                log = log.Replace("\n", "");
            }
            else
            {
                int indent = fc - baseStack.Value;
                if (indent > 0)
                {
                    for (int i = 0; i < indent; ++i) prefix += "┆       ".Color(GetColor(i));
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
                if (!DebugMode) return null;
                baseStack = null;
                SetString("Console", "");
                return new IceBinaryUtility.LogScope(OnLog);
            }
        }
        #endregion
    }
}
