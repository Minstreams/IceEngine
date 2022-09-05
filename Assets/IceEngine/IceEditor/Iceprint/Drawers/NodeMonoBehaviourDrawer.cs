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
        public override GUIStyle StlGraphNodeBackground => _stlGraphNodeBackground?.Check() ?? (_stlGraphNodeBackground = new GUIStyle("NotificationBackground") { overflow = new RectOffset(8, 8, 8, 8), richText = true, }); GUIStyle _stlGraphNodeBackground;
        public override string GetDisplayName(NodeMonoBehaviour node)
        {
            if (node.target != null) return node.target.GetDrawer().GetDisplayName(node.target);
            if (node.targetType != null) return $"{node.targetType.Name.Color(IceGUIUtility.CurrentThemeColor)} (Missing)";
            return "空组件";
        }
        public override Vector2 GetSizeTitle(NodeMonoBehaviour node)
        {
            if (node.target != null) return node.target.GetDrawer().GetSizeTitle(node.target);
            return new(128, 16);
        }
        public override Vector2 GetSizeBody(NodeMonoBehaviour node)
        {
            if (node.target != null) return node.target.GetDrawer().GetSizeBody(node.target);
            return new(224, 32);
        }
        public override void OnGUI_Title(NodeMonoBehaviour node, Rect rect)
        {
            if (node.target != null)
            {
                node.target.GetDrawer().OnGUI_Title(node.target, rect);
                return;
            }

            StyleBox(rect, StlLabel, GetDisplayName(node).Bold());
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
                IceprintNodeComponent val;
                Space(8);
                if (node.targetType == null)
                {
                    val = _ObjectField(node.target, true);
                }
                else
                {
                    val = (IceprintNodeComponent)EditorGUILayout.ObjectField(node.target, node.targetType, true);
                }

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
                    }
                }
            }
        }

        public override void OnSelect(NodeMonoBehaviour node)
        {
            if (node.target != null) Selection.activeObject = node.target;
        }
    }
}
