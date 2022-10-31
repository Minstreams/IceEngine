﻿using UnityEngine;

using IceEngine;
using IceEngine.IceprintNodes;
using static IceEditor.IceGUI;

namespace IceEditor.Internal
{
    internal class NodeMissingDrawer : Framework.IceprintNodeDrawer<NodeMissing>
    {
        public override string GetDisplayName(NodeMissing node) => $"Missing Node {node.Hashcode}";
        public override void OnGUI_Title(NodeMissing node, Rect rect)
        {
            StyleBox(rect, StlBoldLabel, GetDisplayName(node).Color(Color.red));
        }
        public override Vector2 GetSizeTitle(NodeMissing node) => new(160, 16);
        public override Vector2 GetSizeBody(NodeMissing node)
        {
            int count = Mathf.Max(0, Mathf.Max(node.inports.Count, node.outports.Count) - 1);
            return new(0, count * 16);
        }
        public override void OnGUI_Body(NodeMissing node, Rect rect) { }
    }
}
