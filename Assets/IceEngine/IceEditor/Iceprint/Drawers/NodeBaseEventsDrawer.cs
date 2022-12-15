using UnityEngine;

using IceEngine;
using IceEngine.IceprintNodes;
using static IceEditor.IceGUI;

namespace IceEditor.Internal
{
    internal class NodeBaseEventsDrawer : Framework.IceprintNodeDrawer<NodeBaseEvents>
    {
        public override Vector2 GetSizeBody(NodeBaseEvents node) => new Vector2(192, 32);
        public override void OnGUI_Body(NodeBaseEvents node, Rect rect)
        {
            StyleBox(rect, StlLabelNonWrap, "Base events of MonoBehaviour", true);
        }
    }
}
