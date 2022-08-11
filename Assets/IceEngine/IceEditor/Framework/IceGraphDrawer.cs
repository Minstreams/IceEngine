using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using IceEngine;
using IceEngine.Graph;
using static IceEditor.IceGUI;
using static IceEditor.IceGUIAuto;

namespace IceEditor.Framework
{
    public class IceGraphDrawer
    {
        public IceGraph Graph { get; private set; } = null;
        public string Key { get; private set; } = null;
        public IceGraphDrawer(IceGraph graph, string keyOverride = null)
        {
            Graph = graph;
            Key = keyOverride ?? "GraphView";
        }

        #region Port
        public static void DrawPortLine(Vector2 position, Vector2 target, Vector2 tangent, Color startColor, Color endColor, float width = 1.5f, float edge = 1)
        {
            if (E.type != EventType.Repaint) return;

            Vector2 center = 0.5f * (position + target);
            Color centerColor = 0.5f * (startColor + endColor);

            float tangentLength = Mathf.Clamp(Vector2.Dot(tangent, center - position) * 0.6f, 8, 32);
            Vector2 tangentPoint = position + tangent * tangentLength;

            DrawBezierLine(position, center, tangentPoint, startColor, centerColor, width, edge);
        }

        public IceGraphPort DraggingPort { get; private set; }
        public HashSet<IceGraphPort> AvailablePorts = new();

        public void BeginDragPort(IceGraphPort port)
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
        public void EndDragPort()
        {
            DraggingPort = null;
            AvailablePorts.Clear();
        }
        #endregion

        HashSet<IceGraphNode> selectedNodes = new();

        #region GUI

        /// <summary>
        /// 一个Graph操作区
        /// </summary>
        /// <param name="stlBackGround">背景样式</param>
        /// <param name="gridSize">grid块的尺寸</param>
        public void OnGUI(float gridSize = 32, float defaultScale = 1, float minScale = 0.4f, float maxScale = 4.0f, GUIStyle stlBackGround = null, bool inUtilityWindow = false) => OnGUI(GetRect(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)), gridSize, defaultScale, minScale, maxScale, stlBackGround, inUtilityWindow);

        /// <summary>
        /// 一个Graph操作区
        /// </summary>
        /// <param name="area">工作区Rect</param>
        /// <param name="stlBackGround">背景样式</param>
        /// <param name="gridSize">grid块的尺寸</param>
        public void OnGUI(Rect area, float gridSize = 32, float defaultScale = 1, float minScale = 0.4f, float maxScale = 4.0f, GUIStyle stlBackGround = null, bool inUtilityWindow = false)
        {
            using var viewport = ViewportGrid(Key, area, gridSize, defaultScale, minScale, maxScale, null, IceGUIUtility.CurrentThemeColor * 0.5f, stlBackGround, inUtilityWindow);

            foreach (var node in Graph.nodeList)
            {
                var drawer = node.GetDrawer();
                bool bSelected = selectedNodes.Contains(node);

                // Rect Calculation
                var nodeRect = node.GetArea();
                var titleRect = new Rect(nodeRect.x, nodeRect.y, nodeRect.width, node.GetSizeTitle().y);

                // DoubleClick To Fold
                if (E.type == EventType.MouseDown)
                {
                    if (titleRect.Contains(E.mousePosition))
                    {
                        double t = EditorApplication.timeSinceStartup;
                        if (t - _cache_time > 0.2)
                        {
                            _cache_time = t;
                        }
                        else
                        {
                            _cache_time = 0;
                            // 双击
                            node.folded = !node.folded;
                        }
                    }
                }

                // Drag Control
                {
                    int idDragNode = GetControlID();
                    switch (E.type)
                    {
                        case EventType.MouseDown:
                            if (titleRect.Contains(E.mousePosition) && GUIHotControl == 0 && E.button == 0)
                            {
                                GUIHotControl = idDragNode;
                                _cache_drag = node.position - E.mousePosition;
                                _cache_offset = node.position;
                                E.Use();
                            }
                            break;
                        case EventType.MouseUp:
                            if (GUIHotControl == idDragNode && E.button == 0)
                            {
                                GUIHotControl = 0;
                                E.Use();
                            }
                            break;
                        case EventType.MouseDrag:
                            if (GUIHotControl == idDragNode)
                            {
                                node.position = _cache_drag + E.mousePosition;
                                if (E.shift)
                                {
                                    // 平移操作
                                    var offset = node.position - _cache_offset;
                                    if (Mathf.Abs(offset.x) > Mathf.Abs(offset.y))
                                    {
                                        // 沿x轴平移
                                        node.position.y = _cache_offset.y;
                                    }
                                    else
                                    {
                                        // 沿y轴平移
                                        node.position.x = _cache_offset.x;
                                    }
                                }
                                if (E.control)
                                {
                                    // 网格吸附操作
                                    node.position = node.position.Snap(gridSize);
                                }
                                E.Use();
                            }
                            break;
                        case EventType.Repaint:
                            if (GUIHotControl == idDragNode)
                            {
                                var holderRect = new Rect(nodeRect) { position = _cache_offset };

                                // 在原始位置画一个残影
                                using (GUIColor(Color.white * 0.6f)) StyleBox(holderRect, drawer.StlGraphNodeBackground);

                                // TODO: 复制操作
                                if (E.alt)
                                {
                                    // TODO:在原始位置画一个残影
                                }
                            }
                            break;
                    }
                }

                // Draw
                StyleBox(nodeRect, bSelected ? drawer.StlGraphNodeBackgroundSelected : drawer.StlGraphNodeBackground);
                drawer.OnGUI_Title(node, titleRect);
                if (!node.folded)
                {
                    drawer.OnGUI_Body(node, new Rect(node.position, node.GetSizeBody()).Move(y: node.GetSizeFolded().y));
                }
            }

            int idDragPort = GetControlID();
            int idSelectNode = GetControlID();

            if (GUIHotControl == idDragPort)
            {
                // 拖拽中
                switch (E.type)
                {
                    case EventType.MouseUp:
                        if (E.button != 0) E.Use();
                        break;
                    case EventType.MouseDown:
                        if (E.button == 1) E.Use();
                        break;
                    case EventType.MouseDrag:
                        E.Use();
                        break;
                    case EventType.Repaint:
                        var pos = DraggingPort.GetPos();
                        var tagent = DraggingPort.GetTangent();
                        var color = Graph.GetPortColor(DraggingPort);
                        DrawPortLine(pos, E.mousePosition, tagent, color, color);
                        DrawPortLine(E.mousePosition, pos, -tagent, color, color);
                        break;
                }
            }
            else if (GUIHotControl == idSelectNode)
            {
                // 选择中
                switch (E.type)
                {
                    case EventType.MouseUp:
                        if (E.button == 0)
                        {
                            GUIHotControl = 0;
                            E.Use();
                        }
                        break;
                    case EventType.MouseDrag:
                        selectedNodes.Clear();
                        Rect selectionRect = _cache_drag.RangeRect(E.mousePosition);
                        foreach (var node in Graph.nodeList)
                        {
                            if (node.GetArea().Overlaps(selectionRect)) selectedNodes.Add(node);
                        }
                        E.Use();
                        break;
                    case EventType.Repaint:
                        Vector2 p0 = E.mousePosition;
                        Vector2 p1 = new Vector2(_cache_drag.x, p0.y);
                        Vector2 p2 = new Vector2(p0.x, _cache_drag.y);
                        Handles.DrawLine(_cache_drag, p1);
                        Handles.DrawLine(_cache_drag, p2);
                        Handles.DrawLine(p0, p1);
                        Handles.DrawLine(p0, p2);
                        break;
                }
            }

            foreach (var node in Graph.nodeList)
            {
                foreach (var port in node.inports) DrawLine(port);
                foreach (var port in node.outports) DrawLine(port);
            }
            void DrawLine(IceGraphPort port)
            {
                if (E.type == EventType.Repaint && port.IsConnected)
                {
                    Vector2 pos = port.GetPos();
                    var color = Graph.GetPortColor(port);

                    var tagent = port.GetTangent();
                    if (port is IceGraphInport pin)
                    {
                        foreach (var pp in pin.connectedPorts) DrawPortLine(pos, pp.GetPos(), tagent, color, this.Graph.GetPortColor(pp));
                    }
                    else if (port is IceGraphOutport pout)
                    {
                        foreach (var pp in pout.connectedPorts) DrawPortLine(pos, pp.port.GetPos(), tagent, color, this.Graph.GetPortColor(pp.port));
                    }
                }
            }

            foreach (var node in Graph.nodeList)
            {
                foreach (var port in node.inports) OnGUI_Port(port);
                foreach (var port in node.outports) OnGUI_Port(port);
            }
            void OnGUI_Port(IceGraphPort port)
            {
                if (port.node.folded) return;

                Vector2 pos = port.GetPos();
                var color = Graph.GetPortColor(port);

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
                                if (!port.isMultiple) port.DisconnectAll();
                                BeginDragPort(port);
                                E.Use();
                            }
                            else if (E.button == 1)
                            {
                                // Disconnect port
                                port.DisconnectAll();
                                GUIHotControl = 0;  // 截断viewport的move操作
                                E.Use();
                            }
                        }
                        break;
                    case EventType.MouseUp:
                        if (bHover && GUIHotControl == idDragPort && E.button == 0 && AvailablePorts.Contains(port))
                        {
                            // Connect ports
                            DraggingPort.ConnectTo(port);
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
                                string tType = port.valueType?.Name ?? "void";
                                tType = tType switch
                                {
                                    "Single" => "float",
                                    "Int32" => "int",
                                    _ => tType,
                                };
                                tType = tType.Color(color);
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

            if (GUIHotControl == 0)
            {
                switch (E.type)
                {
                    case EventType.MouseDown:
                        if (E.button == 0)
                        {
                            GUIHotControl = idSelectNode;
                            _cache_drag = E.mousePosition;
                            E.Use();
                        }
                        break;
                }
            }


        }
        #endregion
    }
}