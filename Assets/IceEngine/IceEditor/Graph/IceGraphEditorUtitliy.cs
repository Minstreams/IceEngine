using System;

using UnityEditor;
using UnityEngine;

using IceEngine.Graph;
using IceEditor.Graph.Internal;

namespace IceEditor.Graph
{
    public static class IceGraphEditorUtitliy
    {
        #region Port
        public const float PORT_SIZE = 16;
        public const float PORT_RADIUS = PORT_SIZE * 0.5f;

        public static Vector2 GetPos(this IceGraphPort port)
        {
            var node = port.node;
            Vector2 res = node.position;
            if (node.folded)
            {
                if (port.IsOutport)
                {
                    res.x += node.GetSizeFolded().x;
                    res.y += (node.GetSizeFolded().y * (port.id + 1)) / (node.outports.Count + 1);
                }
                else
                {
                    res.y += (node.GetSizeFolded().y * (port.id + 1)) / (node.inports.Count + 1);
                }
            }
            else
            {
                res.y += port.id * PORT_SIZE + PORT_RADIUS;
                if (port.IsOutport) res.x += node.GetSizeUnfolded().x + PORT_RADIUS;
                else res.x -= PORT_RADIUS;
            }
            return res;
        }
        public static Vector2 GetTangent(this IceGraphPort self) => self.IsOutport ? Vector2.right : Vector2.left;
        #endregion

        #region Node
        public static IceGraphNodeDrawer GetDrawer(this IceGraphNode node) => IceGraphNodeDrawer.GetDrawer(node);
        public static Rect GetArea(this IceGraphNode node) => new(node.position, node.GetSize());
        public static Vector2 GetSize(this IceGraphNode node) => node.folded ? node.GetSizeFolded() : node.GetSizeUnfolded();
        public static Vector2 GetSizeUnfolded(this IceGraphNode node) => new
        (
            Mathf.Max(node.GetSizeBody().x, node.GetSizeFolded().x),
            Mathf.Max(node.GetSizeBody().y + node.GetSizeFolded().y, node.inports.Count * PORT_SIZE, node.outports.Count * PORT_SIZE)
        );
        public static Vector2 GetSizeFolded(this IceGraphNode node) => node.GetSizeTitle();
        public static Vector2 GetSizeTitle(this IceGraphNode node) => node.GetDrawer().GetSizeTitle(node);
        public static Vector2 GetSizeBody(this IceGraphNode node) => node.GetDrawer().GetSizeBody(node);
        #endregion
    }
}