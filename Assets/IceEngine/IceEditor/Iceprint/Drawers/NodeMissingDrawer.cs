using UnityEngine;

using IceEngine;
using IceEngine.IceprintNodes;
using static IceEditor.IceGUI;

namespace IceEditor.Internal
{
    internal class NodeMissingDrawer : Framework.IceprintNodeDrawer<NodeMissing>
    {
        public override string GetDisplayName(NodeMissing node) => $"Missing Node {node.Hashcode}".Color(Color.red);
        public override Vector2 GetSizeBody(NodeMissing node)
        {
            int count = Mathf.Max(0, Mathf.Max(node.inports.Count, node.outports.Count) - 1);
            return new(0, count * 16);
        }
    }
}
