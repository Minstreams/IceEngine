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
            if (node.target.Value != null) return node.target.Value.GetDisplayName();
            if (node.targetType != null) return $"{node.targetType.Name.Color(IceGUIUtility.CurrentThemeColor)} (Missing)";
            return "空组件";
        }
        public override Vector2 GetSizeTitle(NodeMonoBehaviour node)
        {
            if (node.target.Value != null) return node.target.Value.GetSizeTitle();
            return new(128, 16);
        }
        public override Vector2 GetSizeBody(NodeMonoBehaviour node)
        {
            if (node.target.Value != null) return node.target.Value.GetSizeBody();
            return new(224, 32);
        }
        public override void OnGUI_Title(NodeMonoBehaviour node, Rect rect)
        {
            var target = node.target.Value;
            if (target != null)
            {
                target.GetDrawer().OnGUI_Title(target, rect);
                return;
            }

            StyleBox(rect, StlLabel, GetDisplayName(node).Color(IceGUIUtility.CurrentThemeColor).Bold());
        }

        public override void OnGUI_Body(NodeMonoBehaviour node, Rect rect)
        {
            var target = node.target.Value;
            if (target != null)
            {
                target.GetDrawer().OnGUI_Body(target, rect);
                return;
            }

            using (AreaRaw(rect)) using (GUICHECK)
            {
                IceprintNodeComponent val;
                Space(8);
                if (node.targetType == null)
                {
                    val = _ObjectField(target, true);
                }
                else
                {
                    val = (IceprintNodeComponent)EditorGUILayout.ObjectField(target, node.targetType, true);
                }

                if (GUIChanged && val != target)
                {
                    node.target.Value = val;
                    if (val != null)
                    {
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
            var target = node.target.Value;
            if (target != null) Selection.activeObject = target;
        }
    }
}
