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
            return new(Mathf.Max(96, StlTextfield.CalcSize(TempContent(node.message)).x + 8), 24);
        }
        public override Vector2 GetSizeTitle(NodeLogger node)
        {
            if (!node.folded) return base.GetSizeTitle(node);
            return new(StlLabel.CalcSize(TempContent(node.message)).x + 80, 32);
        }
        public override void OnGUI_Title(NodeLogger node, Rect rect)
        {
            if (node.folded)
            {
                using (Area(rect)) using (HORIZONTAL)
                {
                    Label("Logger", StlGraphNodeTitle);
                    using (VERTICAL)
                    {
                        Space();
                        Label(node.message, GUILayout.ExpandWidth(true));
                        Space();
                    }
                }
            }
            else
            {
                base.OnGUI_Title(node, rect);
            }
        }
        public override void OnGUI_Body(NodeLogger node, Rect rect)
        {
            using (Area(rect))
            {
                TextField(ref node.message, StlTextfield);
            }
        }
    }
}
