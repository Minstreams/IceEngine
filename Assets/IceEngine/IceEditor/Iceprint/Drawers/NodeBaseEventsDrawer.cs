using UnityEngine;

using IceEngine;
using IceEngine.IceprintNodes;
using static IceEditor.IceGUI;

namespace IceEditor.Internal
{
    internal class NodeBaseEventsDrawer : Framework.IceprintNodeDrawer<NodeBaseEvents>
    {
        public override Vector2 GetSizeTitle(NodeBaseEvents node) => new Vector2(96, 16);
        public override Vector2 GetSizeBody(NodeBaseEvents node) => new Vector2(115, 48);
        public override void OnGUI_Body(NodeBaseEvents node, Rect rect)
        {
            using (Area(rect)) using (LabelWidth(64))
            {
                Label("Base events of MonoBehaviour");
            }
        }
    }
}
