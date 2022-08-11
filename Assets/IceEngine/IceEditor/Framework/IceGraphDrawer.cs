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
        public IceGraphDrawer(IceGraph graph)
        {
            Graph = graph;
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

        public void BeginDrag(IceGraphPort port)
        {
            DraggingPort = port;

            if (port.IsOutport)
            {
                foreach (var node in Graph.nodeList) foreach (var p in node.inports) if (port.IsConnectableTo(p)) AvailablePorts.Add(p);
            }
            else
            {
                foreach (var node in Graph.nodeList) foreach (var p in node.outports) if (port.IsConnectableTo(p)) AvailablePorts.Add(p);
            }
        }
        public void EndDrag()
        {
            DraggingPort = null;
            AvailablePorts.Clear();
        }
        #endregion


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
            using var viewport = ViewportGrid("GraphView", area, gridSize, defaultScale, minScale, maxScale, null, IceGUIUtility.CurrentThemeColor * 0.5f, stlBackGround, inUtilityWindow);

            foreach (var node in Graph.nodeList) node.GetDrawer().OnGUI(node, gridSize);

            int idDragPort = GetControlID();
            foreach (var node in Graph.nodeList)
            {
                foreach (var port in node.inports) OnGUI_Port(port);
                foreach (var port in node.outports) OnGUI_Port(port);
            }
            void OnGUI_Port(IceGraphPort port)
            {
                Vector2 pos = port.GetPos();
                var color = Graph.GetPortColor(port);

                // Line
                if (E.type == EventType.Repaint && port.IsConnected)
                {
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

                if (!port.node.folded)
                {
                    Rect rPort = pos.ExpandToRect(IceGUIUtility.PORT_RADIUS);
                    switch (E.type)
                    {
                        case EventType.MouseDown:
                            // Drag Control
                            if (GUIHotControl == 0 && E.button == 0 && rPort.Contains(E.mousePosition))
                            {
                                offsetCache = rPort.center;
                                GUIHotControl = idDragPort;
                                if (!port.isMultiple) port.DisconnectAll();
                                BeginDrag(port);
                                E.Use();
                            }
                            break;
                        case EventType.MouseUp:
                            if (GUIHotControl == idDragPort && E.button == 0 && rPort.Contains(E.mousePosition) && AvailablePorts.Contains(port))
                            {
                                DraggingPort.ConnectTo(port);
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

                                            if (rPort.Contains(E.mousePosition))
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

                                    if (rPort.Contains(E.mousePosition))
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
                                void DiscSolid(float radius, Color color)
                                {
                                    radius *= IceGUIUtility.PORT_RADIUS;
                                    using (HandlesColor(color)) Handles.DrawSolidDisc(pos, Vector3.forward, radius);
                                    // 柔化边缘
                                    color.a *= 0.4f;
                                    using (HandlesColor(color)) Handles.DrawWireDisc(pos, Vector3.forward, radius + 0.3f / GUI.matrix[0]);
                                }
                            }
                            break;
                    }

                    // Text
                    float wText = StlGraphPortName.CalcSize(new GUIContent(port.name)).x;
                    Rect rText = new Rect(port.IsOutport ? rPort.xMax : rPort.x - wText, rPort.y, wText, IceGUIUtility.PORT_SIZE);
                    GUI.Label(rText, new GUIContent(port.name, port.valueType?.Name ?? "void"), StlGraphPortName);
                    StyleBox(rText, StlGraphPortName, port.name);
                }
            }

            switch (E.type)
            {
                case EventType.MouseUp:
                    if (GUIHotControl == idDragPort && E.button == 0)
                    {
                        GUIHotControl = 0;
                        EndDrag();
                        E.Use();
                    }
                    break;
                case EventType.MouseDrag:
                    if (GUIHotControl == idDragPort)
                    {
                        E.Use();
                    }
                    break;
                case EventType.Repaint:
                    if (GUIHotControl == idDragPort)
                    {
                        // 正在拖动
                        var pos = DraggingPort.GetPos();
                        var tagent = DraggingPort.GetTangent();
                        var color = Graph.GetPortColor(DraggingPort);
                        DrawPortLine(pos, E.mousePosition, tagent, color, color);
                        DrawPortLine(E.mousePosition, pos, -tagent, color, color);
                    }
                    break;
            }
        }
    }
}