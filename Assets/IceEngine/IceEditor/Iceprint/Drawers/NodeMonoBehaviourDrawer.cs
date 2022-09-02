using UnityEditor;
using UnityEngine;

using IceEngine;
using IceEngine.IceprintNodes;
using IceEngine.Internal;
using IceEditor.Framework;
using static IceEditor.IceGUI;
using static IceEditor.IceGUIAuto;
using IceEngine.Framework;
using System;

namespace IceEditor.Internal
{
    public class NodeMonoBehaviourDrawer : IceprintNodeDrawer<NodeMonoBehaviour>
    {
        public override GUIStyle StlGraphNodeBackground => StlBackground;
        public override Vector2 GetSizeTitle(NodeMonoBehaviour node)
        {
            if (node.target != null) return node.target.GetDrawer().GetSizeTitle(node.target);
            return base.GetSizeTitle(node);
        }
        public override Vector2 GetSizeBody(NodeMonoBehaviour node)
        {
            if (node.target != null) return node.target.GetDrawer().GetSizeBody(node.target);
            return base.GetSizeBody(node);
        }
        public override void OnGUI_Title(NodeMonoBehaviour node, Rect rect)
        {
            if (node.target != null)
            {
                node.target.GetDrawer().OnGUI_Title(node.target, rect);
                return;
            }

            base.OnGUI_Title(node, rect);
        }

        public override void OnGUI_Body(NodeMonoBehaviour node, Rect rect)
        {
            if (node.target != null)
            {
                node.target.GetDrawer().OnGUI_Body(node.target, rect);
                return;
            }


            using (AreaRaw(rect)) using (GUICHECK)
            {
                var val = _ObjectField(node.target, true);

                if (GUIChanged && val != node.target)
                {
                    node.target = val;
                    if (val == null)
                    {
                        node.targetInstanceId = 0;
                    }
                    else
                    {
                        node.targetInstanceId = val.GetInstanceID();
                        Type t = val.GetType();

                        if (t != node.targetType || EditorApplication.isPlayingOrWillChangePlaymode)
                        {
                            node.targetType = t;
                            foreach (var ip in node.inports) ip.DisconnectAll();
                            foreach (var op in node.outports) op.DisconnectAll();
                            node.inports.Clear();
                            node.outports.Clear();
                            node.connectionData.Clear();
                            node.InitializePorts();
                        }
                        node.RefreshDisplayName();
                    }
                }
            }
        }
    }
}
