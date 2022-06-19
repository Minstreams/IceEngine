using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using IceEngine;
using static IceEditor.IceGUI;
using static IceEditor.IceGUIAuto;
using System;

namespace IceEditor
{
    public class IceGraphWindow : IceEditorWindow
    {
        #region 定制
        [MenuItem("测试/测试图")]
        static void OpenWindow() => GetWindow<IceGraphWindow>();
        protected override string Title => "测试图窗口";
        #endregion

        protected override void OnEnable()
        {
            base.OnEnable();
            wantsMouseMove = true;
        }


        public IceGraph g = new IceGraph();


        protected override void OnWindowGUI(Rect position)
        {
            using (DOCK)
            {
                if (IceButton("New Node"))
                {
                    var node = new IceGraphNode();
                    node.AddOutport<float>("testOut1");
                    node.AddOutport<int>("testOut2");
                    node.AddInportMultiple("V");
                    g.AddNode(node);
                }
                Space();
                Label($"Node Count: {g.nodeList.Count}");
            }

        }

        protected override void OnExtraGUI(Rect position)
        {
            // 变量定义
            Rect workspace = position.MoveEdge(top: 20);

            IceGraphUtility.GraphArea(this.position, workspace, g, StlDock);
            if (E.type == EventType.MouseMove) Repaint();
        }

    }


    internal static class IceGraphUtility
    {
        public static void GraphArea(Rect baseScreenRect, Rect area, IceGraph g, GUIStyle stlBackGround = null, float gridSize = 32)
        {
            using var viewport = ViewportGrid("GraphView", baseScreenRect, area, gridSize, gridColor: IceGUIUtility.CurrentThemeColor * 0.5f, styleBackground: stlBackGround);

            foreach (var node in g.nodeList) node.GetDrawer().OnGUI(gridSize);

            IdDragPort = GetControlID();
            foreach (var node in g.nodeList)
            {
                foreach (var port in node.inports) OnGUI_Port(port);
                foreach (var port in node.outports) OnGUI_Port(port);
            }
            switch (E.type)
            {
                case EventType.MouseUp:
                    if (GUIHotControl == IdDragPort && E.button == 0)
                    {
                        GUIHotControl = 0;
                        ClearAvailablePorts();
                        E.Use();
                    }
                    break;
                case EventType.MouseDrag:
                    if (GUIHotControl == IdDragPort)
                    {
                        E.Use();
                    }
                    break;
                case EventType.Repaint:
                    if (GUIHotControl == IdDragPort)
                    {
                        // 正在拖动
                        var pos = OriginPort.GetPos();
                        var tagent = OriginPort.GetTangent();
                        var color = g.GetPortColor(OriginPort);
                        DrawPortLine(pos, E.mousePosition, tagent, color, color);
                        DrawPortLine(E.mousePosition, pos, -tagent, color, color);
                    }
                    break;
            }

            GUI.Label(viewport.ClipRect, $"Scale:{GUI.matrix[0]}");
        }

        #region Drawer
        static IceGraphNodeDrawer baseGraphNodeDrawer = new IceGraphNodeDrawer();
        public static IceGraphNodeDrawer GetGraphNodeDrawer(IceGraphNode node)
        {
            baseGraphNodeDrawer.Target = node;
            return baseGraphNodeDrawer;
        }
        public static IceGraphNodeDrawer GetDrawer(this IceGraphNode self) => GetGraphNodeDrawer(self);
        #endregion

        #region Ports
        public static void DrawPortLine(Vector2 position, Vector2 target, Vector2 tangent, Color startColor, Color endColor, float width = 1.5f, float edge = 1)
        {
            if (E.type != EventType.Repaint) return;

            Handles.DrawLine(Vector3.zero, Vector3.zero);
            Vector2 center = 0.5f * (position + target);
            Color centerCol = 0.5f * (startColor + endColor);

            float tangentLength = Mathf.Clamp(Vector2.Dot(tangent, center - position) * 0.6f, 8, 32);
            Vector2 tangentPoint = position + tangent * tangentLength;

            Vector2 delta = center - position;
            int segment = Mathf.CeilToInt((Mathf.Abs(delta.x) + 2 * Mathf.Abs(delta.y)) * GUI.matrix[0] / 16) + 1;
            float unit = 1.0f / (segment - 1);


            float innerWidth = Mathf.Max(0, width - edge);
            if (innerWidth > 0) DrawLinePart(-innerWidth, 1, innerWidth, 1);
            if (edge > 0)
            {
                DrawLinePart(-innerWidth, 1, -width, 0);
                DrawLinePart(innerWidth, 1, width, 0);
            }

            void DrawLinePart(float offset1, float alpha1, float offset2, float alpha2)
            {
                GL.Begin(GL.TRIANGLE_STRIP);

                Vector2 lp = Vector2.zero;
                Vector2 n = tangent.normalized;
                n = new Vector2(n.y, -n.x);
                for (int i = 0; i < segment; ++i)
                {
                    float t = i * unit;
                    var p1 = Vector2.LerpUnclamped(position, tangentPoint, t);
                    var p2 = Vector2.LerpUnclamped(tangentPoint, center, t);
                    Color col = Color.LerpUnclamped(startColor, centerCol, t);
                    Vector2 p = Vector2.LerpUnclamped(p1, p2, t);
                    if (i > 0)
                    {
                        n = (p - lp).normalized;
                        n = new Vector2(n.y, -n.x);
                    }
                    lp = p;
                    col.a = alpha1;
                    GL.Color(col);
                    GL.Vertex(p + n * offset1);
                    col.a = alpha2;
                    GL.Color(col);
                    GL.Vertex(p + n * offset2);
                }

                GL.End();
            }
        }

        public static Vector2 GetPos(this IceGraphPort self)
        {
            var node = self.node;
            Vector2 res = node.position;
            if (node.folded)
            {
                if (self.isOutport)
                {
                    res.x += node.SizeFolded.x;
                    res.y += (node.SizeFolded.y * (self.portId + 1)) / (node.outports.Count + 1);
                }
                else
                {
                    res.y += (node.SizeFolded.y * (self.portId + 1)) / (node.inports.Count + 1);
                }
            }
            else
            {
                res.y += self.portId * IceGraphPort.PORT_SIZE + IceGraphPort.PORT_RADIUS;
                if (self.isOutport) res.x += node.SizeUnfolded.x + IceGraphPort.PORT_RADIUS;
                else res.x -= IceGraphPort.PORT_RADIUS;
            }
            return res;
        }
        public static Vector2 GetTangent(this IceGraphPort self) => self.isOutport ? Vector2.right : Vector2.left;

        public static int IdDragPort { get; set; } = 0;
        public static IceGraphPort OriginPort { get; private set; }
        public static HashSet<IceGraphPort> AvailablePorts = new();
        public static bool CanConnectPorts(IceGraphPort p1, IceGraphPort p2)
        {
            // port连接的全局条件
            if (p1.isOutport == p2.isOutport) return false;
            if (!p1.isMultiple && p1.IsConnected) return false;
            if (!p2.isMultiple && p2.IsConnected) return false;

            return true;
        }
        public static void GetAvailablePorts(this IceGraphPort self)
        {
            OriginPort = self;
            var graph = self.node.graph;

            foreach (var n in graph.nodeList)
            {
                foreach (var p in n.inports) if (CanConnectPorts(p, self) && graph.IsPortsMatch(p, self)) AvailablePorts.Add(p);
                foreach (var p in n.outports) if (CanConnectPorts(p, self) && graph.IsPortsMatch(p, self)) AvailablePorts.Add(p);
            }
        }
        public static void ClearAvailablePorts()
        {
            OriginPort = null;
            AvailablePorts.Clear();
        }



        static GUIStyle StlGraphPortName => _stlGraphPortName?.Check() ?? (_stlGraphPortName = new GUIStyle("label") { margin = new RectOffset(0, 0, 0, 0), padding = new RectOffset(0, 0, 0, 0), fontSize = 8, alignment = TextAnchor.MiddleCenter, }.Initialize(stl => { stl.normal.textColor = new Color(0.3962264f, 0.3962264f, 0.3962264f); })); static GUIStyle _stlGraphPortName;
        public static void OnGUI_Port(IceGraphPort port)
        {
            Vector2 pos = port.GetPos();

            // Line
            if (E.type == EventType.Repaint && port.IsConnected)
            {
                var tagent = port.GetTangent();
                foreach (var pt in port.targetPortList)
                {
                    DrawPortLine(pos, pt.GetPos(), tagent, port.Color, pt.Color);
                }
            }

            if (!port.node.folded)
            {
                Rect rPort = pos.ExpandToRect(IceGraphPort.PORT_RADIUS);
                switch (E.type)
                {
                    case EventType.MouseDown:
                        // Drag Control
                        if (GUIHotControl == 0 && E.button == 0 && rPort.Contains(E.mousePosition))
                        {
                            offsetCache = rPort.center;
                            GUIHotControl = IdDragPort;
                            if (!port.isMultiple) port.DisconnectAll();
                            port.GetAvailablePorts();
                            E.Use();
                        }
                        break;
                    case EventType.MouseUp:
                        if (GUIHotControl == IdDragPort && E.button == 0 && rPort.Contains(E.mousePosition) && AvailablePorts.Contains(port))
                        {
                            OriginPort.ConnectTo(port);
                        }
                        break;
                    case EventType.Repaint:
                        // Port
                        {
                            var c = port.Color;
                            if (GUIHotControl == IdDragPort)
                            {
                                // 拖拽状态
                                if (OriginPort == port)
                                {
                                    // 被拖拽的 Port
                                    DiscSolid(0.15f, c);
                                }
                                else
                                {
                                    // 其他 Port
                                    if (port.IsConnected)
                                    {
                                        // 连接状态
                                        DiscSolid(0.3f, c);
                                    }

                                    if (AvailablePorts.Contains(port))
                                    {
                                        // 备选状态
                                        DiscWire(0.2f, c * 0.8f);
                                        DiscWire(0.8f, c * 0.7f);

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
                                    DiscSolid(0.3f, c);
                                }
                                else
                                {
                                    // 普通
                                    DiscWire(0.3f, c * 0.8f);
                                }

                                if (rPort.Contains(E.mousePosition))
                                {
                                    // hover
                                    DiscWire(0.6f, c);
                                }
                            }


                            // 画 Port 内圈
                            void DiscWire(float radius, Color color)
                            {
                                radius *= IceGraphPort.PORT_RADIUS;
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
                                radius *= IceGraphPort.PORT_RADIUS;
                                using (HandlesColor(color)) Handles.DrawSolidDisc(pos, Vector3.forward, radius);
                                // 柔化边缘
                                color.a *= 0.4f;
                                using (HandlesColor(color)) Handles.DrawWireDisc(pos, Vector3.forward, radius + 0.3f / GUI.matrix[0]);
                            }
                        }

                        // Text
                        float wText = StlGraphPortName.CalcSize(new GUIContent(port.name)).x;
                        Rect rText = new Rect(port.isOutport ? rPort.xMax : rPort.x - wText, rPort.y, wText, IceGraphPort.PORT_SIZE);
                        StyleBox(rText, StlGraphPortName, port.name);

                        break;
                }
            }
        }
        #endregion
    }

    /// <summary>
    /// 用于绘制一个Node
    /// </summary>
    public class IceGraphNodeDrawer
    {
        #region Public
        public IceGraphNode Target { get; set; }
        public void OnGUI(float gridSize)
        {
            // Rect Calculation
            var nodeRect = Target.Area;
            var titleRect = new Rect(nodeRect.x, nodeRect.y, nodeRect.width, Target.SizeTitle.y);

            // DoubleClick To Fold
            {
                if (E.type == EventType.MouseDown)
                {
                    if (titleRect.Contains(E.mousePosition))
                    {
                        double t = EditorApplication.timeSinceStartup;
                        if (t - timeCache > 0.2)
                        {
                            timeCache = t;
                        }
                        else
                        {
                            timeCache = 0;
                            // 双击
                            Target.folded = !Target.folded;
                        }
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
                            dragCache = Target.position - E.mousePosition;
                            offsetCache = Target.position;
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
                            Target.position = dragCache + E.mousePosition;
                            if (E.shift)
                            {
                                // 平移操作
                                var offset = Target.position - offsetCache;
                                if (Mathf.Abs(offset.x) > Mathf.Abs(offset.y))
                                {
                                    // 沿x轴平移
                                    Target.position.y = offsetCache.y;
                                }
                                else
                                {
                                    // 沿y轴平移
                                    Target.position.x = offsetCache.x;
                                }
                            }
                            if (E.control)
                            {
                                // 网格吸附操作
                                Target.position = Target.position.Snap(gridSize);
                            }
                            E.Use();
                        }
                        break;
                    case EventType.Repaint:
                        if (GUIHotControl == idDragNode)
                        {
                            var holderRect = new Rect(nodeRect) { position = offsetCache };

                            // 在原始位置画一个残影
                            using (GUIColor(Color.white * 0.6f)) StyleBox(holderRect, StlGraphNodeBackground);

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
            StyleBox(nodeRect, StlGraphNodeBackground);
            OnGUI_Title(titleRect);
            if (!Target.folded)
            {
                OnGUI_Body(new Rect(Target.position, Target.SizeBody).Move(y: Target.SizeFolded.y));
            }
        }
        #endregion

        #region Virtual
        protected virtual GUIStyle StlGraphNodeBackground => GetStyle("GraphNodeBackground");
        protected virtual void OnGUI_Title(Rect rect)
        {
            StyleBox(rect, GetStyle("GraphNodeTitle"), "测试标题");
        }
        protected virtual void OnGUI_Body(Rect rect)
        {
            StyleBox(rect, GetStyle("GraphNodeBody"), "测试Body");
        }
        #endregion
    }
}