using UnityEngine;

using IceEngine;
using IceEngine.IceprintNodes;
using static IceEditor.IceGUI;

namespace IceEditor.Internal
{
    internal class NodeBaseEventsDrawer : Framework.IceprintNodeDrawer<NodeBaseEvents>
    {
        public override Vector2 GetSizeBody(NodeBaseEvents node) => new Vector2(128, 36);
        public override void OnGUI_Body(NodeBaseEvents node, Rect rect)
        {
            using (Area(rect))
            {
                Label("Base events of MonoBehaviour");
            }
        }
    }
}
