using UnityEngine;

using IceEngine;
using IceEngine.IceprintNodes;
using static IceEditor.IceGUI;

namespace IceEditor.Internal
{
    internal class NodeLoggerDrawer : Framework.IceprintNodeDrawer<NodeLogger>
    {
        GUIStyle StlTextfield => "textfield";
        public override Vector2 GetSizeBody(NodeLogger node)
        {
            return new(Mathf.Max(96, StlTextfield.CalcSize(TempContent(node.message)).x + 8), 32);
        }
        public override Vector2 GetSizeTitle(NodeLogger node)
        {
            if (!node.folded) return base.GetSizeTitle(node);
            return base.GetSizeTitle(node) + new Vector2(StlLabel.CalcSize(TempContent(node.message)).x + 6, 0);
        }
        public override void OnGUI_Title(NodeLogger node, Rect rect)
        {
            const float titleWidth = 74;
            StyleBox(rect.Resize(titleWidth), StlGraphNodeTitle, GetDisplayName(node), true, node.GetArea().Contains(E.mousePosition));
            if (node.folded) StyleBox(rect.MoveEdge(titleWidth), StlLabel, node.message);
        }
        public override void OnGUI_Body(NodeLogger node, Rect rect)
        {
            using (Area(rect))
            {
                Space(6);
                TextField(ref node.message, StlTextfield);
            }
        }
    }
}
