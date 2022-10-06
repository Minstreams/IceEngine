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
    internal class NodeSubSystemCallerDrawer : IceprintNodeDrawer<NodeSubSystemCaller>
    {
        public override string GetDisplayName(NodeSubSystemCaller node) => "CallSubSystem";
        public override Vector2 GetSizeBody(NodeSubSystemCaller node)
        {
            return new(Mathf.Max(96, StlIce.CalcSize(TempContent(node.method)).x + 12), 24);
        }
        public override Vector2 GetSizeTitle(NodeSubSystemCaller node)
        {
            if (!node.folded) return new(96, 16);
            return new(StlLabel.CalcSize(TempContent(node.method)).x + 58, 22);
        }
        public override void OnGUI_Title(NodeSubSystemCaller node, Rect rect)
        {
            if (node.folded)
            {
                using (AreaRaw(rect.ApplyBorder(-2))) using (HORIZONTAL)
                {
                    Label("Call".Bold(), StlIce);
                    Label(node.method);
                }
            }
            else
            {
                base.OnGUI_Title(node, rect);
            }
        }
        public override void OnGUI_Body(NodeSubSystemCaller node, Rect rect)
        {
            using (Area(rect)) using (LabelWidth(56))
            {
                TextField(ref node.method);
            }
        }
    }
}
