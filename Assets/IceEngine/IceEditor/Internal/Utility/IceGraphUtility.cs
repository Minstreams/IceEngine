using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

using IceEngine;
using IceEngine.Internal;
using static IceEditor.IceGUI;
using static IceEditor.IceGUIAuto;

// 这是个临时文件，还待整理

namespace IceEditor.Internal
{
    internal static class IceGraphUtility
    {

        #region Drawer
        static IceGraphNodeDrawer _baseGraphNodeDrawer = new IceGraphNodeDrawer();
        public static IceGraphNodeDrawer GetGraphNodeDrawer(IceGraphNode node)
        {
            _baseGraphNodeDrawer.Target = node;
            return _baseGraphNodeDrawer;
        }
        public static IceGraphNodeDrawer GetDrawer(this IceGraphNode self) => GetGraphNodeDrawer(self);
        #endregion

        #region Ports
        public static void DrawPortLine(Vector2 position, Vector2 target, Vector2 tangent, Color startColor, Color endColor, float width = 1.5f, float edge = 1)
        {
            if (E.type != EventType.Repaint) return;

            Vector2 center = 0.5f * (position + target);
            Color centerColor = 0.5f * (startColor + endColor);

            float tangentLength = Mathf.Clamp(Vector2.Dot(tangent, center - position) * 0.6f, 8, 32);
            Vector2 tangentPoint = position + tangent * tangentLength;

            DrawBezierLine(position, center, tangentPoint, startColor, centerColor, width, edge);
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

        /// <summary>
        /// 正在操作的端口
        /// </summary>
        public static IceGraphPort EditingPort { get; private set; }
        /// <summary>
        /// 当前可连接的端口
        /// </summary>
        public static HashSet<IceGraphPort> AvailablePorts = new();

        public static void GetAvailablePorts(this IceGraphPort self)
        {
            EditingPort = self;
            var graph = self.node.graph;

            foreach (var n in graph.nodeList)
            {
                var ports = self.isOutport ? n.inports : n.outports;
                foreach (var p in ports) if (CanConnectPorts(p, self) && graph.IsPortsMatch(p, self)) AvailablePorts.Add(p);
            }

            static bool CanConnectPorts(IceGraphPort p1, IceGraphPort p2)
            {
                // port连接的全局条件
                //if (p1.isOutport == p2.isOutport) return false;
                if (!p1.isMultiple && p1.IsConnected) return false;
                if (!p2.isMultiple && p2.IsConnected) return false;

                return true;
            }
        }
        public static void ClearAvailablePorts()
        {
            EditingPort = null;
            AvailablePorts.Clear();
        }
        #endregion
    }
}
