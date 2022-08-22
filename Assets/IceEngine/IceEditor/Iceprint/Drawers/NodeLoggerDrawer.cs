using UnityEditor;
using UnityEngine;

using IceEngine;
using IceEngine.IceprintNodes;
using IceEngine.Internal;
using IceEditor.Framework;
using static IceEditor.IceGUI;
using static IceEditor.IceGUIAuto;
using IceEngine.Framework;

namespace IceEditor.Internal
{
    internal class NodeLoggerDrawer : IceprintNodeDrawer<NodeLogger>
    {
        public override Vector2 GetSizeBody(NodeLogger node)
        {
            return new(Mathf.Max(96, StlIce.CalcSize(TempContent(node.message)).x + 12), 48);
        }
        public override Vector2 GetSizeTitle(NodeLogger node)
        {
            if (!node.folded) return new(96, 16);
            return new(StlLabel.CalcSize(TempContent(node.message)).x + 58, 22);
        }
        public override void OnGUI_Title(NodeLogger node, Rect rect)
        {
            if (node.folded)
            {
                using (AreaRaw(rect.ApplyBorder(-2))) using (HORIZONTAL)
                {
                    Label("Logger".Bold(), StlIce);
                    Label(node.message);
                }
            }
            else
            {
                base.OnGUI_Title(node, rect);
            }
        }
        public override void OnGUI_Body(NodeLogger node, Rect rect)
        {
            using (Area(rect)) using (LabelWidth(56))
            {
                Label("Message");
                TextField(ref node.message);
            }
        }
    }
}
